using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IModeratorService
    {
        public Task<Moderator> AddModerator(AddModeratorRequestDto moderatorDto);
        public Task<Moderator> DeleteModerator(Guid moderatorId);
        public Task<Moderator> UpdateModerator(UpdateModeratorDto dto);
        public Task<Moderator> UpdateModeratorAsAdmin(Guid moderatorId, UpdateModeratorasAdminDto dto);
        public Task<PagedResponseDto<Moderator>> QueryModeratorsPaged(ModeratorQueryDto query);
        public Task<Moderator> GetModeratorByIdAsync(Guid moderatorId);
        public Task<Moderator> GetModeratorByEmailAsync(string email);
        public Task<ModeratorStatsDto> GetModeratorStats(Guid moderatorId);
    }
}