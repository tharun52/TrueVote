
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IVoterService
    {
        public Task<IEnumerable<Voter>> GetAllVotersAsync();
        public Task<Voter> AddVoterAsync(AddVoterRequestDto voterDto);
        
    }
}