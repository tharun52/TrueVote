using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Mappers
{
    public class PollMapper : IPollMapper
    {
        public Poll MapPollRequestDtoToPoll(AddPollRequestDto pollDto)
        {

            return new Poll
            {
                Title = pollDto.Title,
                Description = pollDto.Description,
                StartDate = pollDto.StartDate,
                EndDate = pollDto.EndDate
            };
        }
        public PollFile MapPollRequestDtoToPollFile(AddPollRequestDto pollDto)
        {
            using var memoryStream = new MemoryStream();
            if (pollDto.PollFile == null)
            {
                throw new ArgumentNullException(nameof(pollDto.PollFile), "PollFile cannot be null.");
            }
            pollDto.PollFile.CopyToAsync(memoryStream);
            return new PollFile
            {
                Filename = pollDto.PollFile.FileName,
                FileType = Path.GetExtension(pollDto.PollFile.FileName),
                Content = memoryStream.ToArray()
            };
        }
        public PollOption MapPollOptionRequestDtoToPollOption(string Option)
        {
            return new PollOption
            {
                OptionText = Option,
                VoteCount = 0,
                IsDeleted = false
            };
        }
    }
}