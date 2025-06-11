using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IModeratorService
    {
        public Task<Moderator> AddModerator(AddModeratorRequestDto moderatorDto);
        public Task<Moderator> DeleteModerator(Guid moderatorId);
        public Task<Moderator> UpdateModerator(string username, UpdateModeratorDto dto);
        public Task<PagedResponseDto<Moderator>> QueryModeratorsPaged(ModeratorQueryDto query);
        public Task<Moderator> GetModeratorByIdAsync(Guid moderatorId);
        public Task<Moderator> GetModeratorByEmailAsync(string email);
    }
}