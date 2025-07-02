using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<Guid, Admin> _adminRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<Guid, VoterCheck> _voterCheckRepository;
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IRepository<Guid, Poll> _pollRepository;
        private readonly IRepository<Guid, Moderator> _moderatorRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IConfiguration _configuration;
        private readonly IAuditLogger _auditLogger;
        private readonly IAuditService _auditService;

        public AdminService(IRepository<Guid, Admin> adminRepository,
                            IRepository<string, User> userRepository,
                            IRepository<Guid, VoterCheck> voterCheckRepository,
                            IRepository<Guid, Voter> voterRepository,
                            IRepository<Guid, Poll> pollRepository,
                            IRepository<Guid, Moderator> moderatorRepository,                    
                            IEncryptionService encryptionService,
                            IConfiguration configuration,
                            IAuditLogger auditLogger,
                            IAuditService auditService)
        {
            _adminRepository = adminRepository;
            _userRepository = userRepository;
            _voterCheckRepository = voterCheckRepository;
            _voterRepository = voterRepository;
            _pollRepository = pollRepository;
            _moderatorRepository = moderatorRepository;
            _encryptionService = encryptionService;
            _configuration = configuration;
            _auditLogger = auditLogger;
            _auditService = auditService;
        }

        public async Task<AdminStatsDto> GetAdminStats()
        {
            var votes = await _voterCheckRepository.GetAll();
            var voters = await _voterRepository.GetAll();
            var moderators = await _moderatorRepository.GetAll();
            var polls = await _pollRepository.GetAll();

            return new AdminStatsDto
            {
                TotalModeratorRegistered = moderators.Count(),
                TotalPollsCreated = polls.Count(),
                TotalVotersREgistered = voters.Count(),
                TotalVotesVoted = votes.Count()
            };
        }
        public async Task<Admin> AddAdmin(AddAdminRequestDto adminDto)
        {
            if (adminDto == null)
            {
                throw new Exception("Admin DTO cannot be null.");
            }
            if (adminDto.SeceretAdminKey != _configuration["AdminSettings:SecretAdminKey"])
            {
                throw new UnauthorizedAccessException("Invalid secret admin key.");
            }
            var newAdmin = new Admin
            {
                Name = adminDto.Name,
                Email = adminDto.Email
            };

            var encryptedData = await _encryptionService.EncryptData(new EncryptModel
            {
                Data = adminDto.Password
            });
            if (encryptedData == null || encryptedData.EncryptedText == null || encryptedData.HashKey == null)
            {
                throw new InvalidOperationException("Encryption failed: Encrypted data is null.");
            }

            if (await _userRepository.Get(adminDto.Email) != null)
            {
                throw new InvalidOperationException("A user with this email already exists.");
            }

            var user = new User
            {
                Username = adminDto.Email,
                PasswordHash = encryptedData.EncryptedText,
                HashKey = encryptedData.HashKey,
                Role = "Admin",
            };
            var admin = await _adminRepository.Add(newAdmin);

            user.UserId = admin.Id;
            await _userRepository.Add(user);

            await _auditService.LogAsync(
                description: $"Admin added with email: {newAdmin.Email}",
                entityId: newAdmin.Id
            );

            _auditLogger.LogAction(user.Username, $"New Admin with Email : {user.Username} was created", true);
            return admin;
        }

        public async Task<Admin> UpdateAdmin(string email, string prevPassword, string? newPassword, string? name)
        {
            var user = await _userRepository.Get(email);
            if (user == null)
                throw new Exception("User not found");

            var allAdmins = await _adminRepository.GetAll();
            var admin = allAdmins.FirstOrDefault(a => a.Email.Equals(user.Username, StringComparison.OrdinalIgnoreCase));
            if (admin == null)
                throw new Exception("Admin not found");

            if (!string.IsNullOrEmpty(prevPassword))
            {
                if (!BCrypt.Net.BCrypt.Verify(prevPassword, user.PasswordHash))
                    throw new UnauthorizedAccessException("Previous password does not match");

                if (!string.IsNullOrWhiteSpace(newPassword))
                {
                    var encryptedNew = await _encryptionService.EncryptData(new EncryptModel
                    {
                        Data = newPassword
                    });

                    if (encryptedNew == null || encryptedNew.EncryptedText == null || encryptedNew.HashKey == null)
                        throw new InvalidOperationException("Encryption failed for new password");

                    user.PasswordHash = encryptedNew.EncryptedText;
                    user.HashKey = encryptedNew.HashKey;
                    await _userRepository.Update(user.Username, user);
                }
            }

            if (!string.IsNullOrWhiteSpace(name))
                admin.Name = name;

            var updatedAdmin = await _adminRepository.Update(admin.Id, admin);

            await _auditService.LogAsync(
                description: $"Admin with email {email} updated",
                entityId: admin.Id
            );
            
            _auditLogger.LogAction(user.Username, $"Updated admin: {admin.Name} : {admin.Id}", true);

            return updatedAdmin;
        }
        public async Task<Admin> GetAdminByIdAsync(Guid adminId)
        {
            var admin = await _adminRepository.Get(adminId);
            if (admin == null)
                throw new Exception("Admin not found");
            return admin;
        }

        public async Task<bool> DeleteAdminAsync(Guid adminId)
        {
            var admin = await _adminRepository.Get(adminId);
            if (admin == null)
                throw new Exception("Admin not found");

            // Remove associated user if exists
            var user = (await _userRepository.GetAll()).FirstOrDefault(u => u.UserId == adminId);
            if (user != null)
            {
                await _userRepository.Delete(user.Username);
            }

            await _adminRepository.Delete(adminId);

            await _auditService.LogAsync(
                    description: $"Admin with ID {adminId} deleted",
                    entityId: adminId
                );

            _auditLogger.LogAction("System", $"Admin with ID {adminId} was hard deleted", true);
            return true;
        }
    }
}