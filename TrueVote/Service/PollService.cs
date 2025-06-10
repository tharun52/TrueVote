using System.Security.Claims;
using TrueVote.Interfaces;
using TrueVote.Mappers;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class PollService : IPollService
    {
        private readonly IRepository<Guid, Poll> _pollRepository;
        private readonly IRepository<Guid, PollOption> _pollOptionRepository;

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogger _auditLogger;
        private readonly IPollMapper _pollMapper;
        public PollService(IRepository<Guid, Poll> pollRepository,
                           IRepository<Guid, PollOption> pollOptionRepository,
                           IHttpContextAccessor httpContextAccessor,
                           IAuditLogger auditLogger)
        {
            _pollRepository = pollRepository;
            _pollOptionRepository = pollOptionRepository;
            _httpContextAccessor = httpContextAccessor;
            _auditLogger = auditLogger;
            _pollMapper = new PollMapper();
        }

        public async Task<List<PollResponseDto>> ViewAllPolls()
        {
            var polls = await _pollRepository.GetAll();
            var pollOptions = await _pollOptionRepository.GetAll();

            var response = new List<PollResponseDto>();

            foreach (var poll in polls)
            {
                if (!poll.IsDeleted)
                {
                    var optionsForPoll = pollOptions
                        .Where(opt => opt.PollId == poll.Id)
                        .ToList();

                    response.Add(new PollResponseDto
                    {
                        Poll = poll,
                        PollOptions = optionsForPoll
                    });
                }
            }
            if (response.Count == 0)
            {
                throw new Exception("No Poll found in the database");
            }
            return response;
        }
        public async Task<PollResponseDto?> ViewPollById(Guid pollId)
        {
            var poll = await _pollRepository.Get(pollId);
            if (poll == null || !poll.IsDeleted)
                throw new Exception("No Poll with the given Id");

            var pollOptions = await _pollOptionRepository.GetAll();
            var optionsForPoll = pollOptions
                .Where(opt => opt.PollId == poll.Id)
                .ToList();

            return new PollResponseDto
            {
                Poll = poll,
                PollOptions = optionsForPoll
            };
        }
        public async Task<List<PollResponseDto>> ViewPollsByUploadedByUsername(string username)
        {
            var polls = await _pollRepository.GetAll();
            var pollOptions = await _pollOptionRepository.GetAll();

            var userPolls = polls.Where(p => p.CreatedByEmail == username).ToList();

            var response = new List<PollResponseDto>();
            foreach (var poll in userPolls)
            {
                if (!poll.IsDeleted)
                {
                    var optionsForPoll = pollOptions
                        .Where(opt => opt.PollId == poll.Id)
                        .ToList();

                    response.Add(new PollResponseDto
                    {
                        Poll = poll,
                        PollOptions = optionsForPoll
                    });
                }
            }
            if (response.Count == 0)
            {
                throw new Exception($"No Poll found uploaded by the user : {username}");
            }
            return response;
        }

        public async Task<Poll> AddPoll(AddPollRequestDto pollRequestDto)
        {
            var newPoll = _pollMapper.MapPollRequestDtoToPoll(pollRequestDto);
            if (newPoll == null)
            {
                throw new ArgumentException("Poll Dto cannot be null", nameof(pollRequestDto));
            }
            newPoll.IsDeleted = false;
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("You must be Logged in to create a poll");
            }
            newPoll.CreatedByEmail = loggedInUser;

            if (pollRequestDto.PollFile != null)
            {
                newPoll.PoleFile = _pollMapper.MapPollRequestDtoToPollFile(pollRequestDto);
                newPoll.PoleFile.UploadedByUsername = loggedInUser;
            }
            newPoll = await _pollRepository.Add(newPoll);
            foreach (var option in pollRequestDto.OptionTexts)
            {
                var pollOption = _pollMapper.MapPollOptionRequestDtoToPollOption(option);
                pollOption.PollId = newPoll.Id;
                await _pollOptionRepository.Add(pollOption);
            }
            _auditLogger.LogAction(loggedInUser, $"Added a new poll with ID: {newPoll.Id}", true);
            return newPoll;
        }
    }
}