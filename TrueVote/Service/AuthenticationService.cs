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

        public AuthenticationService(ITokenService tokenService,
                                    IEncryptionService encryptionService,
                                    IRepository<string, User> userRepository)
        {
            _tokenService = tokenService;
            _encryptionService = encryptionService;
            _userRepository = userRepository;
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
            var token = await _tokenService.GenerateToken(dbUser);
            return new UserLoginResponse
            {
                UserId = dbUser.UserId,
                Username = user.Username,
                Token = token,
                Role = dbUser.Role
            };
        }
    }
}