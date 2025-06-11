
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IPollMapper
    {
        public Poll MapPollRequestDtoToPoll(AddPollRequestDto pollDto);
        public PollFile MapPollRequestDtoToPollFile(AddPollRequestDto pollDto);
        public PollOption MapPollOptionRequestDtoToPollOption(string Option);
        public PollFile MapPollUpdateDtoToPollFile(UpdatePollRequestDto dto, Guid pollId);
    }
}