
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IPollMapper
    {
        public Poll MapPollRequestDtoToPoll(AddPollRequestDto pollDto);
        public PollFile MapPollRequestDtoToPollFile(AddPollRequestDto pollDto);
        public PollOption MapPollOptionRequestDtoToPollOption(string Option);
        // public Task<PollFile?> MapPollUpdateDtoToPollFileAsync(UpdatePollRequestDto dto, string uploadedByUsername);
    }
}