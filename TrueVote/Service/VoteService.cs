using System.Security.Claims;
using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using TrueVote.Repositories;

namespace TrueVote.Service
{
    public class VoteService : IVoteService
    {
        private readonly IRepository<Guid, VoterCheck> _voterCheckRepository;
        private readonly IRepository<Guid, PollVote> _pollVoteRepository;
        private readonly IRepository<Guid, PollOption> _pollOptionRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRepository<Guid, Voter> _voterRepository;

        public VoteService(IRepository<Guid, VoterCheck> voterCheckRepository,
                           IRepository<Guid, PollVote> pollVoteRepository,
                           IRepository<Guid, PollOption> pollOptionRepository,
                           IHttpContextAccessor httpContextAccessor,
                           IRepository<Guid, Voter> voterRepository)
        {
            _voterCheckRepository = voterCheckRepository;
            _pollVoteRepository = pollVoteRepository;
            _pollOptionRepository = pollOptionRepository;
            _httpContextAccessor = httpContextAccessor;
            _voterRepository = voterRepository;
        }

        public async Task<PollVote> AddVoteAsync(Guid pollOptionId)
        {
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("You must be logged in to vote");
            }
            var voters = await _voterRepository.GetAll();
            var voter = voters.FirstOrDefault(v => v.Email == loggedInUser);
            if (voter == null)
            {
                throw new Exception("The logged in email is not registered as votter");
            }
            var voterId = voter.Id;

            var pollOptions = await _pollOptionRepository.GetAll();
            var currentPollOption = pollOptions.FirstOrDefault(po => po.Id == pollOptionId);
            if (currentPollOption == null)
            {
                throw new Exception("Invalid Poll option");
            }

            var voterChecks = await _voterCheckRepository.GetAll();
            var existingVoterCheck = voterChecks.FirstOrDefault(vc => vc.VoterId == voterId && vc.PollId == currentPollOption.PollId);

            if (existingVoterCheck != null)
            {
                throw new Exception($"This Voter {loggedInUser} has already voted in this poll");    
            }

            var voterCheck = new VoterCheck
            {
                VoterId = voterId,
                PollId = currentPollOption.PollId,
                HasVoted = true,
                VotedAt = DateTime.UtcNow
            };

            voterCheck = await _voterCheckRepository.Add(voterCheck);

            var vote = new PollVote
            {
                PollOptionId = currentPollOption.Id,
                Timestamp = DateTime.UtcNow
            };

            currentPollOption.VoteCount += 1;
            currentPollOption = await _pollOptionRepository.Update(pollOptionId, currentPollOption);

            return await _pollVoteRepository.Add(vote);
        }
    }
}