using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Mappers
{
    public class VoterMapper
    {
        public Voter MapAddVoterRequestDtoToVoter(AddVoterRequestDto voterDto)
        {
            return new Voter
            {
                Name = voterDto.Name,
                Email = voterDto.Email,
                Age = voterDto.Age,
            };
        }
    }
}