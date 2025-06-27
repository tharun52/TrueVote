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
        public PollFile MapPollRequestDtoToPollFile(AddPollRequestDto dto)
        {
            if (dto.PollFile == null)
                throw new ArgumentNullException(nameof(dto.PollFile));

            using var memoryStream = new MemoryStream();
            dto.PollFile.CopyTo(memoryStream);

            return new PollFile
            {
                Id = Guid.NewGuid(),
                Filename = dto.PollFile.FileName,
                FileType = dto.PollFile.ContentType,
                Content = memoryStream.ToArray(),
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
        // public async Task<PollFile?> MapPollUpdateDtoToPollFileAsync(UpdatePollRequestDto dto, string uploadedByUsername)
        // {
        //     if (dto.PollFile == null)
        //         return null;

        //     using var ms = new MemoryStream();
        //     await dto.PollFile.CopyToAsync(ms);

        //     return new PollFile
        //     {
        //         Id = Guid.NewGuid(),
        //         Filename = dto.PollFile.FileName,
        //         FileType = dto.PollFile.ContentType,
        //         Content = ms.ToArray(),
        //         UploadedByUsername = uploadedByUsername,
        //         UploadedAt = DateTime.UtcNow,
        //         IsDeleted = false
        //     };
        // }

    }
}