using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
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
        private readonly IRepository<Guid, PollFile> _pollFileRepository;
        private IRepository<Guid, VoterCheck> _voterCheckRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMessageService _messageService;
        private readonly IAuditLogger _auditLogger;
        private readonly AppDbContext _appDbContext;
        private readonly IAuditService _auditService;
        private readonly IPollMapper _pollMapper;

        public PollService(IRepository<Guid, Poll> pollRepository,
                           IRepository<Guid, PollOption> pollOptionRepository,
                           IRepository<Guid, PollFile> pollFileRepository,
                           IRepository<Guid, VoterCheck> voterCheckRepository,
                           IMessageService messageService,
                           IHttpContextAccessor httpContextAccessor,
                           IAuditLogger auditLogger,
                           AppDbContext appDbContext,
                           IAuditService auditService)
        {
            _pollRepository = pollRepository;
            _pollOptionRepository = pollOptionRepository;
            _pollFileRepository = pollFileRepository;
            _voterCheckRepository = voterCheckRepository;
            _httpContextAccessor = httpContextAccessor;
            _messageService = messageService;
            _auditLogger = auditLogger;
            _appDbContext = appDbContext;
            _auditService = auditService;
            _pollMapper = new PollMapper();
        }



        public async Task<Poll> UpdatePoll(Guid pollId, UpdatePollRequestDto updateDto)
        {
            var poll = await _pollRepository.Get(pollId)
                ?? throw new Exception("Poll not found");

            var loggedInUserEmail = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(loggedInUserEmail))
                throw new UnauthorizedAccessException("No user is logged in.");

            if (!poll.CreatedByEmail.Equals(loggedInUserEmail, StringComparison.OrdinalIgnoreCase))
                throw new UnauthorizedAccessException("Only the creator of the poll can update it.");

            // Title
            if (!string.IsNullOrWhiteSpace(updateDto.Title))
                poll.Title = updateDto.Title;
            else if (updateDto.Title != null)
                throw new ArgumentException("Title cannot be empty");

            // Description
            if (updateDto.Description != null)
                poll.Description = updateDto.Description;

            // Dates
            if (updateDto.StartDate.HasValue)
                poll.StartDate = updateDto.StartDate.Value;

            if (updateDto.EndDate.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                if (updateDto.EndDate <= today)
                    throw new ArgumentException("Poll end date must be greater than today");

                poll.EndDate = updateDto.EndDate.Value;
            }

            if (updateDto.StartDate.HasValue && updateDto.EndDate.HasValue &&
                updateDto.StartDate > updateDto.EndDate)
                throw new ArgumentException("Start date cannot be after end date");

            // IsDeleted
            if (updateDto.IsDeleted.HasValue)
                poll.IsDeleted = updateDto.IsDeleted.Value;

            // Upload Poll File
            if (updateDto.PollFile != null)
            {
                using var memoryStream = new MemoryStream();
                await updateDto.PollFile.CopyToAsync(memoryStream);

                var newPollFile = new PollFile
                {
                    Id = Guid.NewGuid(),
                    Filename = updateDto.PollFile.FileName,
                    FileType = updateDto.PollFile.ContentType,
                    Content = memoryStream.ToArray(),
                    UploadedByUsername = loggedInUserEmail,
                };

                await _pollFileRepository.Add(newPollFile);
                poll.PoleFileId = newPollFile.Id; // Only store the ID
            }

            // Update Poll Options
            if (updateDto.OptionTexts != null)
            {
                if (updateDto.OptionTexts.Count < 2)
                    throw new ArgumentException("At least two poll options are required");

                var distinctOptions = updateDto.OptionTexts
                    .Select(o => o.Trim().ToLowerInvariant())
                    .ToHashSet();

                if (distinctOptions.Count != updateDto.OptionTexts.Count)
                {
                    throw new ArgumentException("Poll options must be unique");
                }
                
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

            await _auditService.LogAsync(
                description: $"Poll updated: {poll.Title}",
                entityId: poll.Id,
                updatedBy: loggedInUserEmail
            );

            _auditLogger.LogAction(loggedInUserEmail, $"Updated poll with ID: {poll.Id}", true);

            return await _pollRepository.Update(poll.Id, poll);
        }


        public async Task<Poll> AddPoll(AddPollRequestDto pollRequestDto)
        {
            if (pollRequestDto == null)
            {
                throw new ArgumentException("Poll request cannot be null");
            }
            if (string.IsNullOrWhiteSpace(pollRequestDto.Title))
            {
                throw new ArgumentException("Poll title is required");
            }
            if (pollRequestDto.OptionTexts == null || pollRequestDto.OptionTexts.Count < 2)
            {
                throw new ArgumentException("At least two poll options are required");
            }

            // Check for duplicate options (case-insensitive, trimmed)
            var distinctOptions = pollRequestDto.OptionTexts
                .Select(o => o.Trim().ToLowerInvariant())
                .ToHashSet();

            if (distinctOptions.Count != pollRequestDto.OptionTexts.Count)
            {
                throw new ArgumentException("Poll options must be unique");
            }

            if (pollRequestDto.EndDate <= DateOnly.FromDateTime(DateTime.Today))
            {
                throw new ArgumentException("Poll end date must be greater than today");
            }

            var newPoll = _pollMapper.MapPollRequestDtoToPoll(pollRequestDto);
            if (newPoll == null)
            {
                throw new ArgumentException("Poll Dto could not be mapped", nameof(pollRequestDto));
            }

            newPoll.IsDeleted = false;

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("You must be logged in to create a poll");
            }

            newPoll.CreatedByEmail = loggedInUser;

            if (pollRequestDto.PollFile != null)
            {
                using var memoryStream = new MemoryStream();
                await pollRequestDto.PollFile.CopyToAsync(memoryStream);

                var pollFile = new PollFile
                {
                    Filename = pollRequestDto.PollFile.FileName,
                    FileType = pollRequestDto.PollFile.ContentType,
                    Content = memoryStream.ToArray(),
                    UploadedByUsername = loggedInUser,
                };

                await _pollFileRepository.Add(pollFile);
                newPoll.PoleFileId = pollFile.Id;
            }

            newPoll = await _pollRepository.Add(newPoll);

            foreach (var option in pollRequestDto.OptionTexts)
            {
                var pollOption = _pollMapper.MapPollOptionRequestDtoToPollOption(option);
                pollOption.PollId = newPoll.Id;
                await _pollOptionRepository.Add(pollOption);
            }

            var pollOptions = (await _pollOptionRepository.GetAll())
                .Where(opt => opt.PollId == newPoll.Id)
                .ToList();

            var pollResponse = new PollResponseDto
            {
                Poll = newPoll,
                PollOptions = pollOptions
            };
        
            if (pollRequestDto.ForPublishing)
            {
                var msg = $"{loggedInUser} has created a new poll: {newPoll.Title}";

                var messageDto = new MessageRequestDto
                {
                    Msg = msg,
                    PollId = newPoll.Id,
                    To = null 
                };

                await _messageService.AddMessage(messageDto);
            }

            await _auditService.LogAsync(
                    description: $"Poll created: {newPoll.Title}",
                    entityId: newPoll.Id,
                    createdBy: loggedInUser
                );
            _auditLogger.LogAction(loggedInUser, $"Added a new poll with ID: {newPoll.Id}", true);

            return newPoll;
        }

        public async Task<bool> DeletePollAsync(Guid pollId)
        {
            var poll = await _pollRepository.Get(pollId);
            if (poll == null)
                throw new Exception("Poll not found");

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("You must be logged in to create a poll");
            }
            if (poll.CreatedByEmail != loggedInUser)
            {
                throw new Exception("You can only delete your polls");
            }

            poll.IsDeleted = true;
            await _pollRepository.Update(poll.Id, poll);

            await _auditService.LogAsync(
                description: $"Poll deleted: {poll.Title}",
                entityId: poll.Id,
                updatedBy: _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
            );

            await _auditService.LogAsync(
                description: $"Deleted Poll updated: {poll.Title}",
                entityId: poll.Id,
                updatedBy: loggedInUser
            );

            _auditLogger.LogAction(loggedInUser, $"Deleted poll with ID: {poll.Id}", true);
            return true;
        }
        public async Task<PollResponseDto> GetPollByIdAsync(Guid pollId)
        {
            var poll = await _pollRepository.Get(pollId);
            if (poll == null || poll.IsDeleted)
                throw new Exception("Poll not found");

            var pollOptions = (await _pollOptionRepository.GetAll())
                .Where(opt => opt.PollId == poll.Id)
                .ToList();

            return new PollResponseDto
            {
                Poll = poll,
                PollOptions = pollOptions,
            };
        }




        public async Task<PagedResponseDto<PollResponseDto>> QueryPollsPaged(PollQueryDto query)
        {
            var polls = (await _pollRepository.GetAll()).ToList();
            var pollOptions = await _pollOptionRepository.GetAll();

            List<VoterCheck>? voterChecks = null;
            if (query.VoterId.HasValue)
            {
                voterChecks = (await _voterCheckRepository.GetAll())
                    .Where(vc => vc.VoterId == query.VoterId.Value)
                    .ToList();
            }
            
            polls = await FilterPolls(polls, query);

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

                    DateTime? voteTime = null;
                    if (voterChecks != null)
                    {
                        var voterCheck = voterChecks.FirstOrDefault(vc => vc.PollId == poll.Id);
                        voteTime = voterCheck?.VotedAt;
                    }

                    response.Add(new PollResponseDto
                    {
                        Poll = poll,
                        PollOptions = optionsForPoll,
                        VoteTime = voteTime
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


        private async Task<List<Poll>> FilterPolls(List<Poll> polls, PollQueryDto query)
        {
            if (!string.IsNullOrEmpty(query.CreatedByEmail))
                polls = polls.Where(p => p.CreatedByEmail == query.CreatedByEmail).ToList();

            if (query.StartDateFrom.HasValue)
                polls = polls.Where(p => p.StartDate >= query.StartDateFrom.Value).ToList();

            if (query.StartDateTo.HasValue)
                polls = polls.Where(p => p.StartDate <= query.StartDateTo.Value).ToList();

            if (query.VoterId.HasValue)
            {
                var voterChecks = await _voterCheckRepository.GetAll();
                var votedPollIds = voterChecks
                    .Where(vc => vc.VoterId == query.VoterId.Value)
                    .Select(vc => vc.PollId)
                    .Distinct()
                    .ToHashSet();

                if (query.ForVoting == false)
                {
                    // if Voter has voted return those polls
                    polls = polls.Where(p => votedPollIds.Contains(p.Id)).ToList();
                }
                else
                {
                    // if Voter hasn't voted  exclude those polls
                    polls = polls.Where(p => !votedPollIds.Contains(p.Id)).ToList();
                }
            }

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
                return polls.OrderByDescending(p => p.StartDate).ToList();

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