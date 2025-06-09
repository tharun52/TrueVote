using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IPollService
    {
        public Task<Poll> AddPoll(AddPollOptionDto addPollOptionDto);
    }
}