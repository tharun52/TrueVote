
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IVoterService
    {
        public Task<IEnumerable<Voter>> GetAllVoters();
        public Task<Voter> AddVoter(AddVoterRequestDto voterDto);
        public Task<Voter> DeleteVoter(Guid voterId);
        public Task<Voter> UpdateVoterAsAdmin(Guid voterId, UpdateVoterAsAdminDto dto);
        public Task<Voter> UpdateVoter(UpdateVoterDto dto);
    }
}