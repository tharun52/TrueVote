using TrueVote.Interfaces;
using TrueVote.Mappers;
using TrueVote.Models;
using TrueVote.Models.DTOs;
using TrueVote.Repositories;

namespace TrueVote.Service
{
    public class VoterService : IVoterService
    {
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly VoterMapper _voterMapper;

        public VoterService(IRepository<Guid, Voter> voterRepository,
                            IRepository<string, User> userRepository,
                            IEncryptionService encryptionService)
        {
            _voterRepository = voterRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _voterMapper = new VoterMapper();
        }

        public async Task<IEnumerable<Voter>> GetAllVotersAsync()
        {
            return await _voterRepository.GetAll();
        }
        public async Task<Voter> AddVoterAsync(AddVoterRequestDto voterDto)
        {
            if (voterDto == null)
            {
                throw new ArgumentException(nameof(voterDto), "Voter Dto cannot be null");
            }
            if (string.IsNullOrEmpty(voterDto.Email) || string.IsNullOrEmpty(voterDto.Password))
            {
                throw new ArgumentException("Email and Password cannot be null or empty.");
            }
            var newVoter = _voterMapper.MapAddVoterRequestDtoToVoter(voterDto);
            if (newVoter.Age < 18)
            {
                throw new ArgumentException("Voter must be at least 18 years old.");
            }

            var encryptedData = await _encryptionService.EncryptData(new EncryptModel
            {
                Data = voterDto.Password
            });
            if (encryptedData == null || encryptedData.EncryptedText == null || encryptedData.HashKey == null)
            {
                throw new InvalidOperationException("Encryption failed: Encrypted data is null.");
            }

            if (await _userRepository.Get(voterDto.Email) != null)
            {
                throw new InvalidOperationException($"A user with this email{voterDto.Email} already exists.");
            }

            var user = new User
            {
                Username = voterDto.Email,
                PasswordHash = encryptedData.EncryptedText,
                HashKey = encryptedData.HashKey,
                Role = "Voter"
            };
            var voter = await _voterRepository.Add(newVoter);
            user.UserId = voter.Id;
            await _userRepository.Add(user);
            return voter;
        }
    }
}