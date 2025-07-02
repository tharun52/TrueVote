using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IAdminService
    {
        public Task<AdminStatsDto> GetAdminStats();
        public Task<Admin> AddAdmin(AddAdminRequestDto adminDto);
        public Task<Admin> GetAdminByIdAsync(Guid adminId);
        public Task<bool> DeleteAdminAsync(Guid adminId);
        public Task<Admin> UpdateAdmin(string email, string prevPassword, string? newPassword, string? name);

    }
}