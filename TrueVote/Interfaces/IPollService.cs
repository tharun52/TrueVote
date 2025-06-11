using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IPollService
    {
        public Task<Poll> AddPoll(AddPollRequestDto addPollOptionDto);
        public Task<PagedResponseDto<PollResponseDto>> QueryPollsPaged(PollQueryDto query);
        public Task<Poll> UpdatePoll(Guid pollId, UpdatePollRequestDto updateDto);
        public Task<PollResponseDto> GetPollByIdAsync(Guid pollId);
    }
}