using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Moq;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Service;
using Xunit;
using System.Collections.Generic;
using System.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

public class TokenServiceTests
{
    private readonly TokenService _tokenService;
    private readonly Mock<IRepository<Guid, RefreshToken>> _refreshTokenRepositoryMock;
    private readonly IConfiguration _configuration;
    private readonly User _testUser;

    public TokenServiceTests()
    {
        _refreshTokenRepositoryMock = new Mock<IRepository<Guid, RefreshToken>>();

        var configDict = new Dictionary<string, string>
        {
            { "Keys:JwtTokenKey", "ThisIsASecretKeyThatShouldBeLongEnough" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configDict)
            .Build();

        _tokenService = new TokenService(_configuration, _refreshTokenRepositoryMock.Object);

        _testUser = new User
        {
            UserId = Guid.NewGuid(),
            Username = "test@example.com",
            Role = "Voter"
        };
    }

    [Fact]
    public async Task GenerateToken_ShouldReturnValidJwtToken()
    {
        // Act
        var token = await _tokenService.GenerateToken(_testUser);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(token));

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        var nameIdentifierClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.NameIdentifier || c.Type == "nameid");

        Assert.NotNull(nameIdentifierClaim);
        Assert.Equal(_testUser.Username, nameIdentifierClaim.Value);

        var roleClaim = jwtToken.Claims.FirstOrDefault(c =>
            c.Type == ClaimTypes.Role || c.Type == "role");

        Assert.NotNull(roleClaim);
        Assert.Equal("Voter", roleClaim.Value);
    }

    [Fact]
    public async Task GenerateTokensAsync_ShouldReturnAccessAndRefreshTokens_AndSaveRefreshToken()
    {
        // Act
        var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(_testUser);

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(accessToken));
        Assert.False(string.IsNullOrWhiteSpace(refreshToken));

        _refreshTokenRepositoryMock.Verify(repo => repo.Add(It.Is<RefreshToken>(
            r => r.Username == _testUser.Username &&
                 r.Token == refreshToken &&
                 r.ExpiresAt > DateTime.UtcNow.AddDays(6))), Times.Once);
    }

    [Fact]
    public async Task RefreshToken_ShouldBe_Base64String_And_64BytesLong()
    {
        // Act
        var (_, refreshToken) = await _tokenService.GenerateTokensAsync(_testUser);

        // Assert
        var raw = Convert.FromBase64String(refreshToken);
        Assert.Equal(64, raw.Length);
    }
}
