using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IAuthenticationService
    {
        public Task<UserLoginResponse> Login(UserLoginRequest user);     
    }
}