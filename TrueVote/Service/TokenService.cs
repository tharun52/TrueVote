using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TrueVote.Interfaces;
using TrueVote.Models;

namespace TrueVote.Service
{
    public class TokenService : ITokenService
    {
        private readonly IRepository<Guid, RefreshToken> _refreshTokenRepository;
        private readonly SymmetricSecurityKey _securityKey;
        
        public TokenService(IConfiguration configuration, IRepository<Guid, RefreshToken> refreshTokenRepository)
        {
            _refreshTokenRepository = refreshTokenRepository;
            _securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Keys:JwtTokenKey"]));
        }

        public Task<string> GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim>
            {
                new Claim("UserId", user.UserId.ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Role, user.Role ?? string.Empty),
            };
            var creds = new SigningCredentials(_securityKey, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Task.FromResult(tokenHandler.WriteToken(token));
        }
        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user)
        {
            var accessToken = await GenerateToken(user);

            var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));

            var expires = DateTime.UtcNow.AddDays(7);
            var rt = new RefreshToken {
                Username = user.Username,
                Token = refreshToken,
                ExpiresAt = expires
            };
            await _refreshTokenRepository.Add(rt);

            return (accessToken, refreshToken);
        }
    }
}