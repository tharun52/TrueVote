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
        private readonly IEncryptionService _encryptionService;

        public ModeratorService(IRepository<Guid, Moderator> moderatorRepository,
                                IRepository<string, User> userRepository,
                                IEncryptionService encryptionService)
        {
            _moderatorRepository = moderatorRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _moderatorMapper = new ModeratorMapper();
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
            user = await _userRepository.Add(user);

            newModerator.IsDeleted = false;
            return await _moderatorRepository.Add(newModerator);
        }
        public async Task<IEnumerable<Moderator>> GetAllModeratorsAsync()
        {
            var moderators = await _moderatorRepository.GetAll();
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
