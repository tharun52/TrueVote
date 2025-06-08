using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IAdminService
    {
        public Task<Admin> AddAdmin(AddAdminRequestDto adminDto);
    }
}