using System.Security.Claims;
using TrueVote.Interfaces;
using TrueVote.Mappers;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class VoterService : IVoterService
    {
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IEncryptionService _encryptionService;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditLogger _auditLogger;
        private readonly VoterMapper _voterMapper;

        public VoterService(IRepository<Guid, Voter> voterRepository,
                            IRepository<string, User> userRepository,
                            IEncryptionService encryptionService,
                            IHttpContextAccessor httpContextAccessor,
                            IAuditLogger auditLogger)
        {
            _voterRepository = voterRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _httpContextAccessor = httpContextAccessor;
            _auditLogger = auditLogger;
            _voterMapper = new VoterMapper();
        }

        public async Task<IEnumerable<Voter>> GetAllVotersAsync()
        {
            var voters = await _voterRepository.GetAll();
            return voters.Where(v => v.IsDeleted == false);
        }
        public async Task<Voter> UpdateVoterAsync(string email, UpdateVoterDto dto)
        {
            var user = await _userRepository.Get(email);
            if (user == null)
                throw new Exception("User not found");

            var voter = (await _voterRepository.GetAll()).FirstOrDefault(v => v.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
            if (voter == null)
                throw new Exception("Voter not found");

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }

            if (!string.IsNullOrEmpty(dto.PrevPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(dto.PrevPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Previous password does not match");

                if (!string.IsNullOrWhiteSpace(dto.NewPassword))
                {
                    var encryptedNew = await _encryptionService.EncryptData(new EncryptModel
                    {
                        Data = dto.NewPassword
                    });

                    if (encryptedNew == null || encryptedNew.EncryptedText == null || encryptedNew.HashKey == null)
                        throw new InvalidOperationException("Encryption failed for new password");

                    user.PasswordHash = encryptedNew.EncryptedText;
                    user.HashKey = encryptedNew.HashKey;
                    await _userRepository.Update(user.Username, user);
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
                voter.Name = dto.Name;

            if (dto.Age.HasValue)
                voter.Age = dto.Age.Value;

            if (dto.IsDeleted.HasValue)
                voter.IsDeleted = dto.IsDeleted.Value;

            _auditLogger.LogAction(loggedInUser, $"Updated Voter: {voter.Name} : {voter.Id}", true);

            return await _voterRepository.Update(voter.Id, voter);
        }

        public async Task<Voter> DeleteVoterAsync(Guid voterId)
        {
            var voter = await _voterRepository.Get(voterId);
            if (voter == null)
            {
                throw new Exception("Voter not found for deletion");
            }
            voter.IsDeleted = true;

            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }
            _auditLogger.LogAction(loggedInUser, $"Soft Deleted Voter : {voter.Id} : {voter.Id}", true);

            return await _voterRepository.Update(voter.Id, voter);
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