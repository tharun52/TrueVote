using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<UserLoginResponse> Login(UserLoginRequest user);
        public Task<UserLoginResponse> RefreshLogin(string refreshToken);
        public Task<bool> LogoutAsync(string refreshToken);
        public Task<User?> GetCurrentUserAsync(string username);
        public Task SendMagicLinkAsync(MagicLinkRequest request);
        public Task<UserLoginResponse> VerifyMagicLinkAsync(MagicLinkVerifyRequest request);

    }
}