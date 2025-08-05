using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
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
        private readonly IRepository<Guid, Moderator> _moderatorRepository;
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IRepository<Guid, MagicLoginToken> _magicTokenRepository;
        private readonly IConfiguration _config;
        private readonly SmtpClient _smtpClient;


        public AuthenticationService(ITokenService tokenService,
                                    IEncryptionService encryptionService,
                                    IRepository<string, User> userRepository,
                                    IRepository<Guid, Moderator> moderatorRepository,
                                    IRepository<Guid, Voter> voterRepository,
                                    IRepository<Guid, RefreshToken> refreshTokenRepository,
                                    IRepository<Guid, MagicLoginToken> magicTokenRepository,
                                    IConfiguration config)
        {
            _tokenService = tokenService;
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _refreshTokenRepository = refreshTokenRepository;
            _moderatorRepository = moderatorRepository;
            _voterRepository = voterRepository;
            _magicTokenRepository = magicTokenRepository;
            _config = config;
            var sender = _config["EmailSettings:Sender"];
            var password = _config["EmailSettings:Password"];
            var host = _config["EmailSettings:SmtpHost"];

            _smtpClient = new SmtpClient(host)
            {
                Port = 587,
                Credentials = new NetworkCredential(sender, password),
                EnableSsl = true
            };
        }

        public async Task<UserLoginResponse> VerifyMagicLinkAsync(MagicLinkVerifyRequest request)
        {
            var token = await _magicTokenRepository
                .GetAll()
                .ContinueWith(t => t.Result.FirstOrDefault(x =>
                    x.Email == request.Email &&
                    x.Token == request.Token &&
                    !x.IsUsed &&
                    x.ExpiresAt > DateTime.UtcNow));

            if (token == null)
                throw new UnauthorizedAccessException("Invalid or expired token.");

            token.IsUsed = true;
            await _magicTokenRepository.Update(token.Id, token);

            var user = await _userRepository.Get(request.Email);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            var (accessToken, refreshToken) = await _tokenService.GenerateTokensAsync(user);
            return new UserLoginResponse
            {
                UserId = user.UserId,
                Username = user.Username,
                Token = accessToken,
                RefreshToken = refreshToken,
                Role = user.Role
            };
        }

        public async Task SendMagicLinkAsync(MagicLinkRequest request)
        {
            var user = await _userRepository.Get(request.Email);
            if (user == null)
                return;

            var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
            var token = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(rawToken));

            var expires = DateTime.UtcNow.AddMinutes(15);
            var magicToken = new MagicLoginToken
            {
                Email = user.Username,
                Token = token,
                ExpiresAt = expires
            };

            await _magicTokenRepository.Add(magicToken);

            var link = $"{request.ClientURI}?email={user.Username}&token={token}";
            var subject = "Magic Login Link";
            var body = $"<p>Click to log in:</p><a href='{link}'>Login</a>";

            var message = new MailMessage("your_email@gmail.com", user.Username, subject, body);
            message.IsBodyHtml = true;

            await _smtpClient.SendMailAsync(message);
        }

        public async Task<UserLoginResponse> Login(UserLoginRequest user)
        {
            var dbUser = await _userRepository.Get(user.Username);
            if (dbUser == null)
            {
                throw new Exception("No such user");
            }

            // Check if the user account is soft-deleted based on role(only moderator and voter, admin does not have isdeleted)
            switch (dbUser.Role.ToLower())
            {
                case "voter":
                    var voter = await _voterRepository.Get(dbUser.UserId);
                    if (voter?.IsDeleted == true)
                        throw new Exception("Voter account is diabled");
                    break;

                case "moderator":
                    var moderator = await _moderatorRepository.Get(dbUser.UserId);
                    if (moderator?.IsDeleted == true)
                        throw new Exception("Moderator account is diabled");
                    break;
                case "admin":
                    break;
                default:
                    throw new Exception("Unknown user role");
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
                throw new UnauthorizedAccessException("Invalid or expired refresh token.");

            var user = await _userRepository.Get(existingToken.Username);
            if (user == null)
                throw new UnauthorizedAccessException("User not found.");

            existingToken.IsRevoked = true;
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



        public async Task<bool> LogoutAsync(string refreshToken)
        {
            var token = (await _refreshTokenRepository.GetAll())
                .FirstOrDefault(r => r.Token == refreshToken && !r.IsRevoked);

            if (token == null)
                return false;

            token.IsRevoked = true;
            await _refreshTokenRepository.Update(token.Id, token);
            return true;
        }

        public async Task<User?> GetCurrentUserAsync(string username)
        {
            if (string.IsNullOrEmpty(username))
                return null;
            return await _userRepository.Get(username);
        }

    }
}