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
    public class PollServiceTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private IHttpContextAccessor GetHttpContextAccessor(string username = "user@test.com")
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.NameIdentifier, username)
            };
            var ctx = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock")) };
            var mock = new Mock<IHttpContextAccessor>();
            mock.Setup(a => a.HttpContext).Returns(ctx);
            return mock.Object;
        }

        private PollService GetService(
            AppDbContext db,
            IHttpContextAccessor accessor,
            Mock<IAuditService> auditService = null)
        {
            var pollRepo = new PollRepository(db);
            var optionRepo = new PollOptionRepository(db);
            var auditLogger = new Mock<IAuditLogger>();
            var auditSvc    = auditService ?? new Mock<IAuditService>();
            return new PollService(
                pollRepo,
                optionRepo,
                accessor,
                auditLogger.Object,
                db,
                auditSvc.Object
            );
        }

        [Fact]
        public async Task AddPoll_Success()
        {
            // Arrange
            var db       = GetDbContext(nameof(AddPoll_Success));
            var accessor = GetHttpContextAccessor("creator@test.com");
            var service  = GetService(db, accessor);
            var dto = new AddPollRequestDto {
                Title       = "Favorite Fruit?",
                Description = "Choose one",
                StartDate   = DateOnly.FromDateTime(DateTime.Today),
                EndDate     = DateOnly.FromDateTime(DateTime.Today.AddDays(2)),
                OptionTexts = new List<string> { "Apple", "Banana", "Cherry" }
            };

            // Act
            var created = await service.AddPoll(dto);

            // Assert
            Assert.NotNull(created);
            Assert.Equal(dto.Title, created.Title);
            Assert.Equal("creator@test.com", created.CreatedByEmail);


            var opts = db.PollOptions.Where(o => o.PollId == created.Id).ToList();
            Assert.Equal(3, opts.Count);
            Assert.Contains(opts, o => o.OptionText == "Banana");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task AddPoll_Throws_WhenTitleEmpty(string badTitle)
        {
            var db       = GetDbContext(nameof(AddPoll_Throws_WhenTitleEmpty));
            var accessor = GetHttpContextAccessor();
            var service  = GetService(db, accessor);
            var dto = new AddPollRequestDto {
                Title       = badTitle,
                OptionTexts = new List<string> { "A", "B" },
                EndDate     = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.AddPoll(dto));
        }

        [Fact]
        public async Task AddPoll_Throws_WhenNotLoggedIn()
        {
            var db       = GetDbContext(nameof(AddPoll_Throws_WhenNotLoggedIn));
            var accessor = new Mock<IHttpContextAccessor>().Object; // HttpContext = null
            var service  = GetService(db, accessor);
            var dto = new AddPollRequestDto {
                Title       = "Q?",
                OptionTexts = new List<string> { "X", "Y" },
                EndDate     = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };

            await Assert.ThrowsAsync<Exception>(() => service.AddPoll(dto));
        }

        [Fact]
        public async Task AddPoll_Throws_WhenTooFewOptions()
        {
            var db       = GetDbContext(nameof(AddPoll_Throws_WhenTooFewOptions));
            var accessor = GetHttpContextAccessor();
            var service  = GetService(db, accessor);
            var dto = new AddPollRequestDto {
                Title       = "Single option?",
                OptionTexts = new List<string> { "OnlyOne" },
                EndDate     = DateOnly.FromDateTime(DateTime.Today.AddDays(1))
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.AddPoll(dto));
        }

        [Fact]
        public async Task AddPoll_Throws_WhenEndDateNotFuture()
        {
            var db       = GetDbContext(nameof(AddPoll_Throws_WhenEndDateNotFuture));
            var accessor = GetHttpContextAccessor();
            var service  = GetService(db, accessor);
            var dto = new AddPollRequestDto {
                Title       = "Past EndDate?",
                OptionTexts = new List<string> { "A", "B" },
                EndDate     = DateOnly.FromDateTime(DateTime.Today) 
            };

            await Assert.ThrowsAsync<ArgumentException>(() => service.AddPoll(dto));
        }
    }
}
