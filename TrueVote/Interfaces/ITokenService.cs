using TrueVote.Models;

namespace TrueVote.Interfaces
{
    public interface ITokenService
    {
        public Task<string> GenerateToken(User user);
    }
}