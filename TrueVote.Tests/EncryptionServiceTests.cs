using System.Threading.Tasks;
using TrueVote.Service;
using TrueVote.Models;
using Xunit;
using BCrypt.Net;

public class EncryptionServiceTests
{
    private readonly EncryptionService _encryptionService;

    public EncryptionServiceTests()
    {
        _encryptionService = new EncryptionService();
    }

    [Fact]
    public async Task EncryptData_ShouldGenerateHash_AndSalt_WhenNoHashKey()
    {
        // Arrange
        var model = new EncryptModel
        {
            Data = "MySecretPassword"
            // No HashKey provided
        };

        // Act
        var result = await _encryptionService.EncryptData(model);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(result.HashKey));
        Assert.False(string.IsNullOrWhiteSpace(result.EncryptedText));
        Assert.True(BCrypt.Net.BCrypt.Verify(model.Data, result.EncryptedText));
    }

    [Fact]
    public async Task EncryptData_ShouldUseExistingHashKey()
    {
        // Arrange
        var original = "password123";
        var salt = BCrypt.Net.BCrypt.GenerateSalt();
        var expectedHash = BCrypt.Net.BCrypt.HashPassword(original, salt);

        var model = new EncryptModel
        {
            Data = original,
            HashKey = salt
        };

        // Act
        var result = await _encryptionService.EncryptData(model);

        // Assert
        Assert.Equal(salt, result.HashKey);
        Assert.Equal(expectedHash, result.EncryptedText);
    }

    [Fact]
    public async Task EncryptData_ShouldHandle_EmptyInputGracefully()
    {
        // Arrange
        var model = new EncryptModel
        {
            Data = null // or string.Empty
        };

        // Act
        var result = await _encryptionService.EncryptData(model);

        // Assert
        Assert.False(string.IsNullOrEmpty(result.EncryptedText));
        Assert.False(string.IsNullOrEmpty(result.HashKey));
    }

    [Fact]
    public async Task EncryptData_DifferentSalt_ShouldProduceDifferentHashes()
    {
        // Arrange
        var password = "SamePassword";
        var model1 = new EncryptModel { Data = password };
        var model2 = new EncryptModel { Data = password };

        // Act
        var result1 = await _encryptionService.EncryptData(model1);
        var result2 = await _encryptionService.EncryptData(model2);

        // Assert
        Assert.NotEqual(result1.EncryptedText, result2.EncryptedText);
        Assert.NotEqual(result1.HashKey, result2.HashKey);
    }
}
