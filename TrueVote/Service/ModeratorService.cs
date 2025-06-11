using System.Security.Claims;
using Serilog;
using TrueVote.Interfaces;
using TrueVote.Mappers;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class ModeratorService : IModeratorService
    {
        private readonly IRepository<Guid, Moderator> _moderatorRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly ModeratorMapper _moderatorMapper;
        private readonly IAuditLogger _auditLogger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEncryptionService _encryptionService;

        public ModeratorService(IRepository<Guid, Moderator> moderatorRepository,
                                IRepository<string, User> userRepository,
                                IEncryptionService encryptionService,
                                IHttpContextAccessor httpContextAccessor,
                                IAuditLogger auditLogger)
        {
            _moderatorRepository = moderatorRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _moderatorMapper = new ModeratorMapper();
            _httpContextAccessor = httpContextAccessor;
            _auditLogger = auditLogger;
        }

        public async Task<Moderator> DeleteModerator(Guid moderatorId)
        {
            var moderator = await _moderatorRepository.Get(moderatorId);
            if (moderator == null)
            {
                throw new Exception("Moderator not found for deletion");
            }
            moderator.IsDeleted = true;

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }
            _auditLogger.LogAction(loggedInUser, $"Soft Deleted moderator: {moderator.Name} : {moderator.Id}", true);

            return await _moderatorRepository.Update(moderator.Id, moderator);
        }


        
        public async Task<Moderator> UpdateModerator(string username, UpdateModeratorDto dto)
        {
            var user = await _userRepository.Get(username);
            if (user == null)
            {
                throw new Exception("User not found");
            }

            var allModerators = await _moderatorRepository.GetAll();
            var moderator = allModerators.FirstOrDefault(m => m.Email.Equals(user.Username, StringComparison.OrdinalIgnoreCase));

            if (moderator == null)
            {
                throw new Exception("Moderator not found");
            }

            var encryptedPrev = await _encryptionService.EncryptData(new EncryptModel
            {
                Data = dto.PrevPassword
            });

            if (!BCrypt.Net.BCrypt.Verify(dto.PrevPassword, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Previous password does not match");
            }

            var updatedModerator = await _moderatorRepository.Update(moderator.Id, moderator);

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }

            moderator.Name = dto.Name;
            moderator.IsDeleted = dto.IsDeleted;

            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                var encryptedNew = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.NewPassword
                });

                if (encryptedNew == null || encryptedNew.EncryptedText == null || encryptedNew.HashKey == null)
                {
                    throw new InvalidOperationException("Encryption failed for new password");
                }

                if (encryptedNew.EncryptedText != user.PasswordHash)
                {
                    user.PasswordHash = encryptedNew.EncryptedText;
                    user.HashKey = encryptedNew.HashKey;

                    await _userRepository.Update(user.Username, user);
                }
            }


            _auditLogger.LogAction(loggedInUser, $"Updated moderator: {moderator.Name} : {moderator.Id}", true);

            return updatedModerator;
        }



        public async Task<Moderator> AddModerator(AddModeratorRequestDto moderatorDto)
        {
            var newModerator = _moderatorMapper.MapAddModeratorRequestDtoToModerator(moderatorDto);
            if (newModerator == null)
            {
                throw new ArgumentException(nameof(moderatorDto), "Moderator Dto cannot be null");
            }

            var encryptedData = await _encryptionService.EncryptData(new EncryptModel
            {
                Data = moderatorDto.Password
            });
            if (encryptedData == null || encryptedData.EncryptedText == null || encryptedData.HashKey == null)
            {
                throw new InvalidOperationException("Encryption failed: Encrypted data is null.");
            }
            var user = new User
            {
                Username = moderatorDto.Email,
                PasswordHash = encryptedData.EncryptedText,
                HashKey = encryptedData.HashKey,
                Role = "Moderator",
            };

            if (await _userRepository.Get(user.Username) != null)
            {
                throw new Exception("User with this email already exists");
            }

            newModerator.IsDeleted = false;
            var moderator = await _moderatorRepository.Add(newModerator);

            user.UserId = moderator.Id;
            var addedUser = await _userRepository.Add(user);

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }
            _auditLogger.LogAction(loggedInUser, $"Added new moderator: {newModerator.Name} : {moderator.Id}", true);
            return moderator;
        }


        public async Task<Moderator> GetModeratorByIdAsync(Guid moderatorId)
        {
            var moderator = await _moderatorRepository.Get(moderatorId);
            if (moderator == null || moderator.IsDeleted)
            {
                throw new Exception("Moderator not found");
            }
            return moderator;
        }

        public async Task<Moderator> GetModeratorByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty");

            var allModerators = await _moderatorRepository.GetAll();
            var moderator = allModerators.FirstOrDefault(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (moderator == null || moderator.IsDeleted)
                throw new Exception("Moderator not found");

            return moderator;
        }

        public async Task<PagedResponseDto<Moderator>> QueryModeratorsPaged(ModeratorQueryDto query)
        {
            var moderators = (await _moderatorRepository.GetAll()).ToList();

            // Filter
            moderators = FilterModerators(moderators, query);

            // Search
            moderators = SearchModerators(moderators, query);

            // Sort
            moderators = SortModerators(moderators, query);

            int totalRecords = moderators.Count;
            int page = query.Page;
            int pageSize = query.PageSize;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // Pagination
            int skip = (page - 1) * pageSize;
            moderators = moderators.Skip(skip).Take(pageSize).ToList();

            return new PagedResponseDto<Moderator>
            {
                Data = moderators,
                Pagination = new PaginationDto
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                }
            };
        }

        private static List<Moderator> FilterModerators(List<Moderator> moderators, ModeratorQueryDto query)
        {
            if (!string.IsNullOrEmpty(query.Email))
                moderators = moderators.Where(m => m.Email.Equals(query.Email, StringComparison.OrdinalIgnoreCase)).ToList();

            if (query.IsDeleted.HasValue)
                moderators = moderators.Where(m => m.IsDeleted == query.IsDeleted.Value).ToList();

            return moderators;
        }

        private static List<Moderator> SearchModerators(List<Moderator> moderators, ModeratorQueryDto query)
        {
            if (!string.IsNullOrEmpty(query.SearchTerm))
            {
                var term = query.SearchTerm.ToLower();
                moderators = moderators.Where(m =>
                    (!string.IsNullOrEmpty(m.Name) && m.Name.ToLower().Contains(term)) ||
                    (!string.IsNullOrEmpty(m.Email) && m.Email.ToLower().Contains(term))
                ).ToList();
            }
            return moderators;
        }

        private static List<Moderator> SortModerators(List<Moderator> moderators, ModeratorQueryDto query)
        {
            if (string.IsNullOrEmpty(query.SortBy))
                return moderators.OrderBy(m => m.Name).ToList(); // Default sort

            return query.SortBy.ToLower() switch
            {
                "name" => query.SortDesc ? moderators.OrderByDescending(m => m.Name).ToList() : moderators.OrderBy(m => m.Name).ToList(),
                "email" => query.SortDesc ? moderators.OrderByDescending(m => m.Email).ToList() : moderators.OrderBy(m => m.Email).ToList(),
                _ => moderators
            };
        }
    }
}
