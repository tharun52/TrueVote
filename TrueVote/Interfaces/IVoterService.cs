
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IVoterService
    {
        public Task<IEnumerable<Voter>> GetAllVotersAsync();
        public Task<Voter> AddVoterAsync(AddVoterRequestDto voterDto);
        public Task<Voter> UpdateVoterAsync(string email, UpdateVoterDto dto);
        public Task<Voter> DeleteVoterAsync(Guid voterId);
    }
}