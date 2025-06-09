using System.Threading.Tasks;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Mappers
{
    public class PollMapper
    {
        public async Task<Poll> MapPollRequestDtoToPoll(AddPollRequestDto pollDto)
        {
            return new Poll
            {
                Title = pollDto.Title,
                Description = pollDto.Description,
                StartDate = pollDto.StartDate,
                EndDate = pollDto.EndDate
            };
        }
        public async Task<PollFile> MapPollRequestDtoToPollFile(AddPollRequestDto pollDto)
        {
            using var memoryStream = new MemoryStream();
            await pollDto.PollFile.CopyToAsync(memoryStream);
            return new PollFile
            {
                Filename = pollDto.PollFile.FileName,
                FileType = Path.GetExtension(pollDto.PollFile.FileName),
                Content = memoryStream.ToArray()
            };
        }
    }
}