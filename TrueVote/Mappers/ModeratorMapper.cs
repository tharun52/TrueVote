
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Mappers
{
    public class ModeratorMapper
    {
        public Moderator MapAddModeratorRequestDtoToModerator(AddModeratorRequestDto dto)
        {
            if (dto == null) throw new Exception("Give full dto");
            return new Moderator
            {
                Name = dto.Name,
                Email = dto.Email,
                IsDeleted = false
            };
        }
    }
}