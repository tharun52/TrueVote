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
        private readonly IAuditService _auditService;
        private readonly IAuditLogger _auditLogger;
        private readonly VoterMapper _voterMapper;

        public VoterService(IRepository<Guid, Voter> voterRepository,
                            IRepository<string, User> userRepository,
                            IEncryptionService encryptionService,
                            IHttpContextAccessor httpContextAccessor,
                            IAuditService auditService,
                            IAuditLogger auditLogger)
        {
            _voterRepository = voterRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _httpContextAccessor = httpContextAccessor;
            _auditService = auditService;
            _auditLogger = auditLogger;
            _voterMapper = new VoterMapper();
        }

        public async Task<IEnumerable<Voter>> GetAllVoters()
        {
            var voters = await _voterRepository.GetAll();
            return voters.Where(v => v.IsDeleted == false);
        }

        public async Task<Voter> UpdateVoterAsAdmin(Guid voterId, UpdateVoterAsAdminDto dto)
        {
            // 1. Get the voter by ID
            var voter = await _voterRepository.Get(voterId);
            if (voter == null)
                throw new Exception("Voter not found");

            // 2. Update fields only if provided
            if (!string.IsNullOrWhiteSpace(dto.Name))
                voter.Name = dto.Name;

            if (dto.Age.HasValue)
            {
                if (dto.Age.Value < 18)
                    throw new Exception("Voter Must be atleast 18 years old");
                voter.Age = dto.Age.Value;
            }

            if (dto.IsDeleted.HasValue)
                voter.IsDeleted = dto.IsDeleted.Value;

            // 3. Get admin's user ID from JWT (for logging)
            var adminId = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (adminId == null)
            {
                throw new Exception("No User Logged in");
            }
            var loggedInUser = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (loggedInUser == null)
            {
                throw new Exception("No User Logged in");
            }

            // 4. Log admin update action
            await _auditService.LogAsync(
                description: $"Voter deleted: {voter.Email}",
                entityId: voter.Id,
                updatedBy: loggedInUser
            );
            _auditLogger.LogAction(adminId, $"Admin updated voter: {voter.Name} : {voter.Id}", true);

            // 5. Save changes
            return await _voterRepository.Update(voter.Id, voter);
        }

        public async Task<Voter> UpdateVoter(UpdateVoterDto dto)
        {
            // 1. Get Voter ID from JWT (UserId claim)
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid voterId))
            {
                throw new UnauthorizedAccessException("You must be logged in to update Voter");
            }

            // 2. Fetch Voter by ID
            var voter = await _voterRepository.Get(voterId);
            if (voter == null)
                throw new Exception("Voter not found");

            // 3. Fetch associated User by voter's email
            var user = await _userRepository.Get(voter.Email);
            if (user == null)
                throw new Exception("User account not found");

            // 4. Optional password update
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                if (string.IsNullOrWhiteSpace(dto.PrevPassword))
                    throw new UnauthorizedAccessException("Previous password is required to change password");

                if (!BCrypt.Net.BCrypt.Verify(dto.PrevPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Previous password does not match");

                var encryptedNew = await _encryptionService.EncryptData(new EncryptModel
                {
                    Data = dto.NewPassword
                });

                if (encryptedNew == null || string.IsNullOrWhiteSpace(encryptedNew.EncryptedText) || string.IsNullOrWhiteSpace(encryptedNew.HashKey))
                    throw new InvalidOperationException("Encryption failed for new password");

                user.PasswordHash = encryptedNew.EncryptedText;
                user.HashKey = encryptedNew.HashKey;

                await _userRepository.Update(user.Username, user);
            }

            // 5. Update voter fields if present
            if (!string.IsNullOrWhiteSpace(dto.Name))
                voter.Name = dto.Name;

            if (dto.Age.HasValue)
            {
                if (dto.Age.Value < 18)
                    throw new Exception("Voter Must be atleast 18 years old");
                voter.Age = dto.Age.Value;
            }

            if (dto.IsDeleted.HasValue)
                voter.IsDeleted = dto.IsDeleted.Value;

            // 6. Log audit
            _auditLogger.LogAction(userIdClaim, $"Updated Voter: {voter.Name} : {voter.Id}", true);

            return await _voterRepository.Update(voter.Id, voter);
        }

        public async Task<Voter> DeleteVoter(Guid voterId)
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

        public async Task<Voter> AddVoter(AddVoterRequestDto voterDto)
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