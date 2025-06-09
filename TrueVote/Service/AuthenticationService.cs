using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class AuthenticationService : IAuthenticationService
    {
        private readonly ITokenService _tokenService;
        private readonly IEncryptionService _encryptionService;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<Guid, RefreshToken> _refreshTokenRepository;

        public AuthenticationService(ITokenService tokenService,
                                    IEncryptionService encryptionService,
                                    IRepository<string, User> userRepository,
                                    IRepository<Guid, RefreshToken> refreshTokenRepository)
        {
            _tokenService = tokenService;
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
        }
        public async Task<UserLoginResponse> Login(UserLoginRequest user)
        {
            var dbUser = await _userRepository.Get(user.Username);
            if (dbUser == null)
            {
                throw new Exception("No such user");
            }
            var encryptedData = await _encryptionService.EncryptData(new EncryptModel
            {
                Data = user.Password,
                HashKey = dbUser.HashKey
            });
            if (encryptedData.Data == null)
            {
                throw new Exception("Encryption failed");
            }
            if (dbUser.PasswordHash == null)
            {
                throw new Exception("User password is not set");
            }
            if (!BCrypt.Net.BCrypt.Verify(user.Password, dbUser.PasswordHash))
            {
                throw new Exception("Invalid password");
            }
            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(dbUser);

            return new UserLoginResponse
            {
                UserId = dbUser.UserId,
                Username = dbUser.Username,
                Token = accessToken,
                RefreshToken = refreshToken,
                Role = dbUser.Role
            };
        }
        
        public async Task<UserLoginResponse> RefreshLogin(string refreshToken)
        {
            var existingToken = (await _refreshTokenRepository.GetAll())
                .FirstOrDefault(r => r.Token == refreshToken && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow);

            if (existingToken == null)
            {
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");
            }

            var user = await _userRepository.Get(existingToken.Username);
            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            await _refreshTokenRepository.Update(existingToken.Id, existingToken);

            var (accessToken, newRefreshToken) = await _tokenService.GenerateTokensAsync(user);

            return new UserLoginResponse
            {
                Username = user.Username,
                Token = accessToken,
                RefreshToken = newRefreshToken,
                Role = user.Role
            };
        }
    }
}