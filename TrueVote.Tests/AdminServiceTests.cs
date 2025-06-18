using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Moq;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using TrueVote.Service;
using Xunit;

public class AdminServiceTests
{
    private readonly Mock<IRepository<Guid, Admin>> _adminRepoMock = new();
    private readonly Mock<IRepository<string, User>> _userRepoMock = new();
    private readonly Mock<IEncryptionService> _encryptionServiceMock = new();
    private readonly Mock<IConfiguration> _configMock = new();
    private readonly Mock<IAuditLogger> _auditLoggerMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();
    private readonly AdminService _adminService;

    public AdminServiceTests()
    {
        _configMock.Setup(c => c["AdminSettings:SecretAdminKey"]).Returns("secret123");

        _adminService = new AdminService(
            _adminRepoMock.Object,
            _userRepoMock.Object,
            _encryptionServiceMock.Object,
            _configMock.Object,
            _auditLoggerMock.Object,
            _auditServiceMock.Object
        );
    }

    [Fact]
    public async Task AddAdmin_ShouldAddAdmin_WhenValidInput()
    {
        // Arrange
        var request = new AddAdminRequestDto
        {
            Name = "Test Admin",
            Email = "admin@example.com",
            Password = "Password123",
            SeceretAdminKey = "secret123"
        };

        var encrypted = new EncryptModel
        {
            EncryptedText = "encrypted",
            HashKey = "hashkey"
        };

        _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
            .ReturnsAsync(encrypted);

        _userRepoMock.Setup(r => r.Get(request.Email)).ReturnsAsync((User)null);

        var admin = new Admin { Id = Guid.NewGuid(), Name = "Test Admin", Email = "admin@example.com" };
        _adminRepoMock.Setup(r => r.Add(It.IsAny<Admin>())).ReturnsAsync(admin);
        _userRepoMock.Setup(r => r.Add(It.IsAny<User>())).ReturnsAsync((User u) => u);

        // Act
        var result = await _adminService.AddAdmin(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Test Admin", result.Name);
        _auditLoggerMock.Verify(a => a.LogAction(request.Email, It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public async Task UpdateAdmin_ShouldUpdateNameAndPassword_WhenValidInput()
    {
        // Arrange
        var email = "admin@example.com";
        var prevPassword = "oldpass";
        var newPassword = "newpass";
        var name = "Updated Name";

        var user = new User
        {
            Username = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(prevPassword),
            UserId = Guid.NewGuid()
        };

        var admin = new Admin
        {
            Id = user.UserId,
            Email = email,
            Name = "Old Name"
        };

        _userRepoMock.Setup(r => r.Get(email)).ReturnsAsync(user);
        _adminRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<Admin> { admin });

        var encryptedNew = new EncryptModel
        {
            EncryptedText = "newEncrypted",
            HashKey = "newHash"
        };

        _encryptionServiceMock.Setup(e => e.EncryptData(It.IsAny<EncryptModel>()))
            .ReturnsAsync(encryptedNew);

        _userRepoMock.Setup(r => r.Update(user.Username, user)).ReturnsAsync(user);
        _adminRepoMock.Setup(r => r.Update(admin.Id, It.IsAny<Admin>())).ReturnsAsync((Guid id, Admin a) => a);

        // Act
        var result = await _adminService.UpdateAdmin(email, prevPassword, newPassword, name);

        // Assert
        Assert.Equal(name, result.Name);
        _auditLoggerMock.Verify(a => a.LogAction(user.Username, It.IsAny<string>(), true), Times.Once);
    }

    [Fact]
    public async Task GetAdminByIdAsync_ShouldReturnAdmin_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var admin = new Admin { Id = id, Name = "Admin A" };
        _adminRepoMock.Setup(r => r.Get(id)).ReturnsAsync(admin);

        // Act
        var result = await _adminService.GetAdminByIdAsync(id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Admin A", result.Name);
    }

    [Fact]
    public async Task DeleteAdminAsync_ShouldDeleteAdminAndUser_WhenExists()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var admin = new Admin { Id = adminId, Email = "admin@example.com" };
        var user = new User { UserId = adminId, Username = "admin@example.com" };

        _adminRepoMock.Setup(r => r.Get(adminId)).ReturnsAsync(admin);
        _userRepoMock.Setup(r => r.GetAll()).ReturnsAsync(new List<User> { user });

        _userRepoMock.Setup(r => r.Delete(user.Username)).ReturnsAsync(user);
        _adminRepoMock.Setup(r => r.Delete(adminId)).ReturnsAsync(admin);

        // Act
        var result = await _adminService.DeleteAdminAsync(adminId);

        // Assert
        Assert.True(result);
        _auditLoggerMock.Verify(a => a.LogAction("System", It.IsAny<string>(), true), Times.Once);
    }
}
