using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
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
        private readonly AppDbContext _appDbContext;
        private readonly IPollMapper _pollMapper;
        public PollService(IRepository<Guid, Poll> pollRepository,
                           IRepository<Guid, PollOption> pollOptionRepository,
                           IHttpContextAccessor httpContextAccessor,
                           IAuditLogger auditLogger,
                           AppDbContext appDbContext)
        {
            _pollRepository = pollRepository;
            _pollOptionRepository = pollOptionRepository;
            _httpContextAccessor = httpContextAccessor;
            _auditLogger = auditLogger;
            _appDbContext = appDbContext;
            _pollMapper = new PollMapper();
        }

        public async Task<Poll> UpdatePoll(Guid pollId, UpdatePollRequestDto updateDto)
        {
            var poll = await _pollRepository.Get(pollId);
            if (poll == null || poll.IsDeleted)
                throw new Exception("Poll not found");

            var loggedInUserEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(loggedInUserEmail))
                throw new UnauthorizedAccessException("No user is logged in.");

            if (!poll.CreatedByEmail.Equals(loggedInUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Only the ");

            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                poll.Title = updateDto.Title;

            if (updateDto.Description != null)
                poll.Description = updateDto.Description;

            if (updateDto.StartDate.HasValue)
                poll.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
                poll.EndDate = updateDto.EndDate.Value;

            if (updateDto.IsDeleted.HasValue)
                poll.IsDeleted = updateDto.IsDeleted.Value;

            if (updateDto.PollFile != null)
            {
                var newPollFile = _pollMapper.MapPollUpdateDtoToPollFile(updateDto, poll.Id);
                newPollFile.UploadedByUsername = loggedInUserEmail;
                newPollFile.PollId = poll.Id;

                _appDbContext.Entry(poll).State = EntityState.Unchanged;
                _appDbContext.PollFiles.Add(newPollFile);

                poll.PoleFile = newPollFile;
            }

            if (updateDto.OptionTexts != null && updateDto.OptionTexts.Any())
            {
                var existingOptions = (await _pollOptionRepository.GetAll())
                    .Where(o => o.PollId == poll.Id)
                    .ToList();

                foreach (var opt in existingOptions)
                {
                    await _pollOptionRepository.Delete(opt.Id);
                }

                foreach (var optionText in updateDto.OptionTexts)
                {
                    var pollOption = _pollMapper.MapPollOptionRequestDtoToPollOption(optionText);
                    pollOption.PollId = poll.Id;
                    await _pollOptionRepository.Add(pollOption);
                }
            }

            return await _pollRepository.Update(poll.Id, poll);
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

        public async Task<PagedResponseDto<PollResponseDto>> QueryPollsPaged(PollQueryDto query)
        {
            var polls = (await _pollRepository.GetAll()).ToList();
            var pollOptions = await _pollOptionRepository.GetAll();

            polls = FilterPolls(polls, query);

            polls = SearchPolls(polls, query);

            polls = SortPolls(polls, query);

            int totalRecords = polls.Count;
            int page = query.Page;
            int pageSize = query.PageSize;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            int skip = (page - 1) * pageSize;
            polls = polls.Skip(skip).Take(pageSize).ToList();

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

            return new PagedResponseDto<PollResponseDto>
            {
                Data = response,
                Pagination = new PaginationDto
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                }
            };
        }


        private List<Poll> FilterPolls(List<Poll> polls, PollQueryDto query)
        {
            if (!string.IsNullOrEmpty(query.CreatedByEmail))
                polls = polls.Where(p => p.CreatedByEmail == query.CreatedByEmail).ToList();

            if (query.StartDateFrom.HasValue)
                polls = polls.Where(p => p.StartDate >= query.StartDateFrom.Value).ToList();

            if (query.StartDateTo.HasValue)
                polls = polls.Where(p => p.StartDate <= query.StartDateTo.Value).ToList();

            return polls;
        }

        private List<Poll> SearchPolls(List<Poll> polls, PollQueryDto query)
        {
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                polls = polls.Where(p =>
                    (!string.IsNullOrEmpty(p.Title) && p.Title.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(term))
                ).ToList();
            }
            return polls;
        }

        private List<Poll> SortPolls(List<Poll> polls, PollQueryDto query)
        {
            if (string.IsNullOrEmpty(query.SortBy))
                return polls.OrderByDescending(p => p.StartDate).ToList(); // Default sort

            return query.SortBy.ToLower() switch
            {
                "title" => query.SortDesc ? polls.OrderByDescending(p => p.Title).ToList() : polls.OrderBy(p => p.Title).ToList(),
                "startdate" => query.SortDesc ? polls.OrderByDescending(p => p.StartDate).ToList() : polls.OrderBy(p => p.StartDate).ToList(),
                "enddate" => query.SortDesc ? polls.OrderByDescending(p => p.EndDate).ToList() : polls.OrderBy(p => p.EndDate).ToList(),
                _ => polls
            };
        }
    }
}