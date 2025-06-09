using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class AdminService : IAdminService
    {
        private readonly IRepository<Guid, Admin> _adminRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IEncryptionService _encryptionService;
        private readonly IConfiguration _configuration;
        public AdminService(IRepository<Guid, Admin> adminRepository,
                            IRepository<string, User> userRepository,
                            IEncryptionService encryptionService,
                            IConfiguration configuration)
        {
            _adminRepository = adminRepository;
            _userRepository = userRepository;
            _encryptionService = encryptionService;
            _configuration = configuration;
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
            
            return admin;
        }
    }
}