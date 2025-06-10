using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IPollService
    {
        public Task<Poll> AddPoll(AddPollRequestDto addPollOptionDto);
        public Task<List<PollResponseDto>> ViewAllPolls();
        public Task<PollResponseDto?> ViewPollById(Guid pollId);
        public Task<List<PollResponseDto>> ViewPollsByUploadedByUsername(string username);

    }
}