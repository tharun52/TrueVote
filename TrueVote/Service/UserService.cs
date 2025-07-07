
using TrueVote.Interfaces;
using TrueVote.Models;

namespace TrueVote.Service
{
    public class UserService : IUserService
    {
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<Guid, Moderator> _moderatorRepository;
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IModeratorService _moderatorService;
        private readonly IVoterService _voterService;

        public UserService(
            IRepository<string, User> userRepository,
            IRepository<Guid, Moderator> moderatorRepository,
            IRepository<Guid, Voter> voterRepository,
            IModeratorService moderatorService,
            IVoterService voterService
        )
        {
            _userRepository = userRepository;
            _moderatorRepository = moderatorRepository;
            _voterRepository = voterRepository;
            _moderatorService = moderatorService;
            _voterService = voterService;
        }

        public async Task<object> GetUserDetailsByIdAsync(string userId)
        {
            if (string.IsNullOrEmpty(userId) || !Guid.TryParse(userId, out var userGuid))
            {
                throw new ArgumentException("Invalid or missing User ID", nameof(userId));
            }

            var users = await _userRepository.GetAll();
            var user = users.SingleOrDefault(u => u.UserId == userGuid);
            if (user == null)
            {
                throw new KeyNotFoundException("User not found");
            }

            return user.Role switch
            {
                "Moderator" => await _moderatorService.GetModeratorByEmailAsync(user.Username),
                "Voter" => await _voterService.GetVoterByEmail(user.Username),
                _ => throw new InvalidOperationException("Unknown user role")
            };
        }


    }
}