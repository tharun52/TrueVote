using System.ComponentModel.DataAnnotations;
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
        private readonly IRepository<string, VoterEmail> _voterEmailRepository;
        private readonly IEncryptionService _encryptionService;
        private IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditService _auditService;
        private readonly IAuditLogger _auditLogger;
        private readonly VoterMapper _voterMapper;

        public VoterService(IRepository<Guid, Voter> voterRepository,
                            IRepository<string, User> userRepository,
                            IRepository<string, VoterEmail> voterEmailRepository,
                            IEncryptionService encryptionService,
                            IHttpContextAccessor httpContextAccessor,
                            IAuditService auditService,
                            IAuditLogger auditLogger)
        {
            _voterRepository = voterRepository;
            _userRepository = userRepository;
            _voterEmailRepository = voterEmailRepository;
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

        public async Task<IEnumerable<Voter>> GetVotersByModeratorId(Guid moderatorId)
        {
            var voters = await _voterRepository.GetAll();
            return voters.Where(v => v.ModeratorId == moderatorId && !v.IsDeleted);
        }

        public async Task<IEnumerable<VoterEmail>> GetWhiteListedEmailsByModerator(Guid moderatorId)
        {
            var whitelistedEmails = await _voterEmailRepository.GetAll();
            if (whitelistedEmails == null)
            {
                throw new Exception("No emails found");
            }
            whitelistedEmails = whitelistedEmails
                .Where(we => we.ModeratorId == moderatorId)
                .OrderByDescending(we => we.IsUsed);
            if (whitelistedEmails == null)
            {
                throw new Exception("This moderator has not created any emails");
            }
            return whitelistedEmails;
        }

        public async Task<Voter> GetVoterByEmail(string voterEmail)
        {
            var voters = await _voterRepository.GetAll();
            if (voters == null)
            {
                throw new Exception($"No Voters found");
            }
            var voter = voters.FirstOrDefault(v => v.Email == voterEmail);
            if (voter == null)
            {
                throw new Exception($"No Voter found by the Email : ${voterEmail}");
            }
            return voter;
        }
        public async Task<Voter> UpdateVoterAsModerator(Guid voterId, UpdateVoterAsModeratorDto dto)
        {
            // 1. Get the voter by ID
            var voter = await _voterRepository.Get(voterId);
            if (voter == null)
                throw new Exception("Voter not found");

            // 2. Get moderator's user ID from JWT
            var moderatorIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (moderatorIdStr == null || !Guid.TryParse(moderatorIdStr, out Guid moderatorId))
            {
                throw new Exception("No User Logged in");
            }

            // 3. Check if the logged-in moderator created this voter
            if (voter.ModeratorId != moderatorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to update this voter");
            }

            // 4. Update fields if provided
            if (!string.IsNullOrWhiteSpace(dto.Name))
                voter.Name = dto.Name;

            if (dto.Age.HasValue)
            {
                if (dto.Age.Value < 18)
                    throw new Exception("Voter must be at least 18 years old");
                voter.Age = dto.Age.Value;
            }

            if (dto.IsDeleted.HasValue)
                voter.IsDeleted = dto.IsDeleted.Value;

            // 5. Log moderator update action
            _auditLogger.LogAction(moderatorIdStr, $"Moderator updated voter: {voter.Name} : {voter.Id}", true);
            await _auditService.LogAsync(
                description: $"Voter updated by moderator: {voter.Email}",
                entityId: voter.Id,
                updatedBy: moderatorIdStr
            );

            // 6. Save and return
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

            // 4. Optional password update (no need for previous password)
            if (!string.IsNullOrWhiteSpace(dto.NewPassword))
            {
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
                    throw new Exception("Voter must be at least 18 years old");
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

            // Get Moderator ID from token (UserId)
            var userIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out Guid loggedInModeratorId))
            {
                throw new UnauthorizedAccessException("No valid user is logged in");
            }

            // Allow only if this moderator created the voter
            if (voter.ModeratorId != loggedInModeratorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this voter");
            }

            // Soft delete
            voter.IsDeleted = true;
            
            _auditLogger.LogAction(userIdStr, $"Soft Deleted Voter : {voter.Id}", true);

            return await _voterRepository.Update(voter.Id, voter);
        }


        public async Task<List<VoterEmail>> WhitelistVoterEmails(WhitelistVoterEmailDto dto)
        {
            if (dto.Emails == null || dto.Emails.Count == 0)
                throw new ArgumentException("Email list cannot be empty.");

            var moderatorIdStr = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (moderatorIdStr == null || !Guid.TryParse(moderatorIdStr, out Guid moderatorId))
                throw new UnauthorizedAccessException("Invalid moderator login.");

            var addedEmails = new List<VoterEmail>();

            foreach (var email in dto.Emails)
            {
                if (string.IsNullOrWhiteSpace(email) || !new EmailAddressAttribute().IsValid(email))
                    continue;

                var exists = await _voterEmailRepository.Get(email);
                if (exists != null)
                    throw new Exception($"{email} already exists");

                var voterEmail = new VoterEmail
                {
                    Email = email,
                    ModeratorId = moderatorId,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await _voterEmailRepository.Add(voterEmail);
                addedEmails.Add(result);
            }

            return addedEmails;
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

            var whitelisted = await _voterEmailRepository.Get(voterDto.Email);
            if (whitelisted == null)
            {
                throw new UnauthorizedAccessException("You are not authorized to sign up. Please contact your moderator.");
            }


            var newVoter = _voterMapper.MapAddVoterRequestDtoToVoter(voterDto);
            newVoter.ModeratorId = whitelisted.ModeratorId;

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

            //set isUsed
            whitelisted.IsUsed = true;
            await _voterEmailRepository.Update(whitelisted.Email, whitelisted);

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