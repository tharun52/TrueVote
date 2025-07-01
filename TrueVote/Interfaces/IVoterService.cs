
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IVoterService
    {
        public Task<bool> CheckEmail(string email);
        public Task<IEnumerable<Voter>> GetAllVoters();
        public Task<IEnumerable<Voter>> GetVotersByModeratorId(Guid moderatorId);
        public Task<Voter> GetVoterByEmail(string voterEmail);
        public Task<IEnumerable<VoterEmail>> GetWhiteListedEmailsByModerator(Guid moderatorId);
        public Task<List<VoterEmail>> WhitelistVoterEmails(WhitelistVoterEmailDto dto);
        public Task<VoterEmail> DeleteWhitelistEmail(string email);
        public Task<Voter> AddVoter(AddVoterRequestDto voterDto);
        public Task<Voter> DeleteVoter(Guid voterId);
        public Task<Voter> UpdateVoterAsModerator(Guid voterId, UpdateVoterAsModeratorDto dto);
        public Task<Voter> UpdateVoter(UpdateVoterDto dto);
    }
}