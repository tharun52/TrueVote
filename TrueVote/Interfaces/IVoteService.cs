using TrueVote.Models;

namespace TrueVote.Interfaces
{
    public interface IVoteService
    {
        public Task<PollVote> AddVoteAsync(Guid pollOptionId);
        
    }
}