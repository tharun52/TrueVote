using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;
using TrueVote.Contexts;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using TrueVote.Repositories;
using TrueVote.Service;
using Xunit;

namespace TrueVote.Tests
{
    public class ModeratorServiceTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private Moderator GetTestModerator(string name = "Mod", string email = "mod@test.com")
        {
            return new Moderator
            {
                Id = Guid.NewGuid(),
                Name = name,
                Email = email,
                IsDeleted = false
            };
        }

        private User GetTestUser(string email = "mod@test.com", string password = "password123")
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return new User
            {
                UserId = Guid.NewGuid(),
                Username = email,
                PasswordHash = hash,
                HashKey = "dummykey",
                Role = "Moderator"
            };
        }

        private ClaimsPrincipal GetClaimsPrincipal(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, username)
            };
            return new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        }

        private IHttpContextAccessor GetHttpContextAccessor(string username)
        {
            var context = new DefaultHttpContext
            {
                User = GetClaimsPrincipal(username)
            };
            var accessor = new Mock<IHttpContextAccessor>();
            accessor.Setup(a => a.HttpContext).Returns(context);
            return accessor.Object;
        }

        private ModeratorService GetService(
            AppDbContext db,
            IHttpContextAccessor accessor,
            Mock<IEncryptionService> encryptionServiceMock = null)
        {
            var modRepo = new ModeratorRepository(db);
            var userRepo = new UserRepository(db);
            var auditLogger = new Mock<IAuditLogger>();
            var auditService = new Mock<IAuditService>();
            var encryptionService = encryptionServiceMock ?? new Mock<IEncryptionService>();
            return new ModeratorService(
                modRepo,
                userRepo,
                encryptionService.Object,
                accessor,
                auditLogger.Object,
                auditService.Object
            );
        }

        [Fact]
        public async Task AddModerator_Success()
        {
            var db = GetDbContext(nameof(AddModerator_Success));
            var accessor = GetHttpContextAccessor("admin@test.com");

            var encryptionServiceMock = new Mock<IEncryptionService>();
            encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
                .ReturnsAsync(new EncryptModel { EncryptedText = "hash", HashKey = "key" });

            var service = GetService(db, accessor, encryptionServiceMock);

            var dto = new AddModeratorRequestDto
            {
                Name = "Test Mod",
                Email = "mod1@test.com",
                Password = "pass"
            };

            var result = await service.AddModerator(dto);

            Assert.NotNull(result);
            Assert.Equal(dto.Name, result.Name);
            Assert.Equal(dto.Email, result.Email);

            var user = db.Users.FirstOrDefault(u => u.Username == dto.Email);
            Assert.NotNull(user);
            Assert.Equal("Moderator", user.Role);
        }

        [Fact]
        public async Task AddModerator_Throws_WhenUserExists()
        {
            var db = GetDbContext(nameof(AddModerator_Throws_WhenUserExists));
            var accessor = GetHttpContextAccessor("admin@test.com");

            db.Users.Add(new User
            {
                UserId = Guid.NewGuid(),
                Username = "mod1@test.com",
                PasswordHash = "hash",
                HashKey = "key",
                Role = "Moderator"
            });
            db.SaveChanges();

            var encryptionServiceMock = new Mock<IEncryptionService>();
            encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
                .ReturnsAsync(new EncryptModel { EncryptedText = "hash", HashKey = "key" });

            var service = GetService(db, accessor, encryptionServiceMock);

            var dto = new AddModeratorRequestDto
            {
                Name = "Test Mod",
                Email = "mod1@test.com",
                Password = "pass"
            };

            await Assert.ThrowsAsync<Exception>(() => service.AddModerator(dto));
        }

        [Fact]
        public async Task DeleteModerator_SoftDeletes()
        {
            var db = GetDbContext(nameof(DeleteModerator_SoftDeletes));
            var mod = GetTestModerator();
            db.Moderators.Add(mod);
            db.SaveChanges();

            var accessor = GetHttpContextAccessor("admin@test.com");
            var service = GetService(db, accessor);

            var result = await service.DeleteModerator(mod.Id);

            Assert.True(result.IsDeleted);
        }

        [Fact]
        public async Task GetModeratorByIdAsync_ReturnsModerator()
        {
            var db = GetDbContext(nameof(GetModeratorByIdAsync_ReturnsModerator));
            var mod = GetTestModerator();
            db.Moderators.Add(mod);
            db.SaveChanges();

            var accessor = GetHttpContextAccessor("admin@test.com");
            var service = GetService(db, accessor);

            var result = await service.GetModeratorByIdAsync(mod.Id);

            Assert.NotNull(result);
            Assert.Equal(mod.Email, result.Email);
        }

        [Fact]
        public async Task GetModeratorByIdAsync_Throws_WhenDeleted()
        {
            var db = GetDbContext(nameof(GetModeratorByIdAsync_Throws_WhenDeleted));
            var mod = GetTestModerator();
            mod.IsDeleted = true;
            db.Moderators.Add(mod);
            db.SaveChanges();

            var accessor = GetHttpContextAccessor("admin@test.com");
            var service = GetService(db, accessor);

            await Assert.ThrowsAsync<Exception>(() => service.GetModeratorByIdAsync(mod.Id));
        }

        [Fact]
        public async Task GetModeratorByEmailAsync_ReturnsModerator()
        {
            var db = GetDbContext(nameof(GetModeratorByEmailAsync_ReturnsModerator));
            var mod = GetTestModerator();
            db.Moderators.Add(mod);
            db.SaveChanges();

            var accessor = GetHttpContextAccessor("admin@test.com");
            var service = GetService(db, accessor);

            var result = await service.GetModeratorByEmailAsync(mod.Email);

            Assert.NotNull(result);
            Assert.Equal(mod.Name, result.Name);
        }

        [Fact]
        public async Task QueryModeratorsPaged_ReturnsPaged()
        {
            var db = GetDbContext(nameof(QueryModeratorsPaged_ReturnsPaged));
            db.Moderators.Add(new Moderator { Id = Guid.NewGuid(), Name = "A", Email = "a@test.com", IsDeleted = false });
            db.Moderators.Add(new Moderator { Id = Guid.NewGuid(), Name = "B", Email = "b@test.com", IsDeleted = false });
            db.Moderators.Add(new Moderator { Id = Guid.NewGuid(), Name = "C", Email = "c@test.com", IsDeleted = false });
            db.SaveChanges();

            var accessor = GetHttpContextAccessor("admin@test.com");
            var service = GetService(db, accessor);

            var query = new ModeratorQueryDto { Page = 1, PageSize = 2 };
            var result = await service.QueryModeratorsPaged(query);

            Assert.Equal(2, result.Data.Count());
            Assert.Equal(3, result.Pagination.TotalRecords);
            Assert.Equal(2, result.Pagination.PageSize);
            Assert.Equal(2, result.Pagination.TotalPages);
        }
    }
}