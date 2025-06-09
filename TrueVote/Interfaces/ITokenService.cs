using TrueVote.Models;

namespace TrueVote.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
        public Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(User user);
    }
}