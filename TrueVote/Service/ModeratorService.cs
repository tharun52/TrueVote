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
            _auditLogger = auditLogger;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<Moderator> UpdateModerator(Guid moderatorId, AddModeratorRequestDto moderatorDto)
        {
            var moderator = await _moderatorRepository.Get(moderatorId);
            if (moderator == null)
            {
                throw new Exception("Moderator not found for update");
            }

            var user = await _userRepository.Get(moderator.Email);
            if (user == null)
            {
                throw new Exception("Associated user not found");
            }

            moderator.Name = moderatorDto.Name;
            moderator.Email = moderatorDto.Email;

            if (!string.Equals(user.Username, moderatorDto.Email, StringComparison.OrdinalIgnoreCase))
            {
                user.Username = moderatorDto.Email;
            }

            if (!string.IsNullOrWhiteSpace(moderatorDto.Password))
            {
                var encryptedData = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = moderatorDto.Password
                });
                if (encryptedData == null || encryptedData.EncryptedText == null || encryptedData.HashKey == null)
                {
                    throw new InvalidOperationException("Encryption failed: Encrypted data is null.");
                }
                user.PasswordHash = encryptedData.EncryptedText;
                user.HashKey = encryptedData.HashKey;
            }

            await _userRepository.Update(user.Username, user);
            var updatedModerator = await _moderatorRepository.Update(moderatorId, moderator);

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
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


        public async Task<IEnumerable<Moderator>> GetAllModeratorsAsync()
        {
            var moderators = await _moderatorRepository.GetAll();
            moderators = moderators.Where(m => !m.IsDeleted);
            if (moderators == null || !moderators.Any())
            {
                throw new Exception("No Moderators found");
            }
            return moderators;
        }
        public async Task<Moderator> GetModeratorByNameAsync(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("Moderator name cannot be null or empty.", nameof(name));
            }
            var moderators = await _moderatorRepository.GetAll();
            var moderator = moderators.FirstOrDefault(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (moderator == null)
            {
                throw new KeyNotFoundException($"Moderator with name {name} not found.");
            }
            return moderator;
        }
    }
}
