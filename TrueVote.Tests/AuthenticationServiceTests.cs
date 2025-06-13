using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class AuthenticationServiceTests
    {
        private AppDbContext GetDbContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new AppDbContext(options);
        }

        private User GetTestUser(string username = "testuser", string password = "password123", string role = "User")
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return new User
            {
                UserId = Guid.NewGuid(),
                Username = username,
                PasswordHash = hash,
                HashKey = "dummykey",
                Role = role
            };
        }

        private RefreshToken GetTestRefreshToken(string username, string token, bool isRevoked = false, DateTime? expiresAt = null)
        {
            return new RefreshToken
            {
                Id = Guid.NewGuid(),
                Username = username,
                Token = token,
                IsRevoked = isRevoked,
                ExpiresAt = expiresAt ?? DateTime.UtcNow.AddMinutes(10)
            };
        }

        private AuthenticationService GetService(AppDbContext db, Mock<ITokenService> tokenServiceMock, Mock<IEncryptionService> encryptionServiceMock)
        {
            var userRepo = new UserRepository(db);
            var refreshRepo = new RefreshTokenRepository(db);
            return new AuthenticationService(
                tokenServiceMock.Object,
                encryptionServiceMock.Object,
                userRepo,
                refreshRepo
            );
        }

        [Fact]
        public async Task Login_Success_ReturnsUserLoginResponse()
        {
            var db = GetDbContext(nameof(Login_Success_ReturnsUserLoginResponse));
            var user = GetTestUser();
            db.Users.Add(user);
            db.SaveChanges();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(t => t.GenerateTokensAsync(It.IsAny<User>()))
                .ReturnsAsync(("access-token", "refresh-token"));

            var encryptionServiceMock = new Mock<IEncryptionService>();
            encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
                .ReturnsAsync(new EncryptModel { Data = "encrypted" });

            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.Login(new UserLoginRequest
            {
                Username = user.Username,
                Password = "password123"
            });

            Assert.Equal(user.Username, result.Username);
            Assert.Equal("access-token", result.Token);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(user.Role, result.Role);
        }

        [Fact]
        public async Task Login_Fails_InvalidUsername_ThrowsException()
        {
            var db = GetDbContext(nameof(Login_Fails_InvalidUsername_ThrowsException));
            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await service.Login(new UserLoginRequest
                {
                    Username = "nouser",
                    Password = "password"
                });
            });
        }

        [Fact]
        public async Task Login_Fails_InvalidPassword_ThrowsException()
        {
            var db = GetDbContext(nameof(Login_Fails_InvalidPassword_ThrowsException));
            var user = GetTestUser();
            db.Users.Add(user);
            db.SaveChanges();

            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
                .ReturnsAsync(new EncryptModel { Data = "encrypted" });

            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            await Assert.ThrowsAsync<Exception>(async () =>
            {
                await service.Login(new UserLoginRequest
                {
                    Username = user.Username,
                    Password = "wrongpassword"
                });
            });
        }

        [Fact]
        public async Task RefreshLogin_Success_ReturnsNewTokens()
        {
            var db = GetDbContext(nameof(RefreshLogin_Success_ReturnsNewTokens));
            var user = GetTestUser();
            db.Users.Add(user);
            var refreshToken = GetTestRefreshToken(user.Username, "valid-refresh-token");
            db.RefreshTokens.Add(refreshToken);
            db.SaveChanges();

            var tokenServiceMock = new Mock<ITokenService>();
            tokenServiceMock.Setup(t => t.GenerateTokensAsync(It.IsAny<User>()))
                .ReturnsAsync(("new-access-token", "new-refresh-token"));

            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.RefreshLogin("valid-refresh-token");

            Assert.Equal(user.Username, result.Username);
            Assert.Equal("new-access-token", result.Token);
            Assert.Equal("new-refresh-token", result.RefreshToken);
        }

        [Fact]
        public async Task RefreshLogin_Fails_InvalidToken_ThrowsUnauthorized()
        {
            var db = GetDbContext(nameof(RefreshLogin_Fails_InvalidToken_ThrowsUnauthorized));
            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            await Assert.ThrowsAsync<UnauthorizedAccessException>(async () =>
            {
                await service.RefreshLogin("bad-token");
            });
        }

        [Fact]
        public async Task LogoutAsync_Success_ReturnsTrue()
        {
            var db = GetDbContext(nameof(LogoutAsync_Success_ReturnsTrue));
            var user = GetTestUser();
            db.Users.Add(user);
            var refreshToken = GetTestRefreshToken(user.Username, "logout-token");
            db.RefreshTokens.Add(refreshToken);
            db.SaveChanges();

            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.LogoutAsync("logout-token");
            Assert.True(result);

            var updatedToken = db.RefreshTokens.First(r => r.Token == "logout-token");
            Assert.True(updatedToken.IsRevoked);
        }

        [Fact]
        public async Task LogoutAsync_Fails_TokenNotFound_ReturnsFalse()
        {
            var db = GetDbContext(nameof(LogoutAsync_Fails_TokenNotFound_ReturnsFalse));
            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.LogoutAsync("notfound-token");
            Assert.False(result);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsUser()
        {
            var db = GetDbContext(nameof(GetCurrentUserAsync_ReturnsUser));
            var user = GetTestUser();
            db.Users.Add(user);
            db.SaveChanges();

            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.GetCurrentUserAsync(user.Username);
            Assert.NotNull(result);
            Assert.Equal(user.Username, result.Username);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsNull_WhenUserNotFound()
        {
            var db = GetDbContext(nameof(GetCurrentUserAsync_ReturnsNull_WhenUserNotFound));
            var tokenServiceMock = new Mock<ITokenService>();
            var encryptionServiceMock = new Mock<IEncryptionService>();
            var service = GetService(db, tokenServiceMock, encryptionServiceMock);

            var result = await service.GetCurrentUserAsync("nouser");
            Assert.Null(result);
        }
    }
}