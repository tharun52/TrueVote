using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IModeratorService
    {
        public Task<Moderator> AddModerator(AddModeratorRequestDto moderatorDto);
        public Task<IEnumerable<Moderator>> GetAllModeratorsAsync();
        public Task<Moderator> GetModeratorByNameAsync(string name);
        public Task<Moderator> DeleteModerator(Guid moderatorId);
        public Task<Moderator> UpdateModerator(Guid moderatorId, AddModeratorRequestDto moderatorDto);
    }
}