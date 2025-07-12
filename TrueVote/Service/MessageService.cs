using System.Security.Claims;
using TrueVote.Interfaces;
using TrueVote.Mappers;
using TrueVote.Migrations;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Guid, Message> _messageRepository;
        private readonly IRepository<Guid, UserMessage> _userMessageRepository;
        private readonly IRepository<string, User> _userRepository;
        private readonly IRepository<Guid, Voter> _voterRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IConfiguration _configuration;
        private readonly MessageMapper _mesageMapper;

        public MessageService(
            IRepository<Guid, Message> messageRepository,
            IRepository<Guid, UserMessage> userMessageRepository,
            IRepository<string, User> userRepository,
            IRepository<Guid, Voter> voterRepository,
            IHttpContextAccessor httpContextAccessor,
            IConfiguration configuration
        )
        {
            _messageRepository = messageRepository;
            _userMessageRepository = userMessageRepository;
            _userRepository = userRepository;
            _voterRepository = voterRepository;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _mesageMapper = new MessageMapper();
        }

        public async Task<Message> AddMessage(MessageRequestDto messageDto)
        {
            if (string.IsNullOrWhiteSpace(messageDto.Msg))
            {
                throw new Exception("Message cannot be null or empty");
            }

            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (email == null)
            {
                throw new Exception("No user logged in");
            }

            var allUsers = await _userRepository.GetAll();
            var sender = allUsers.SingleOrDefault(u => u.Username == email);

            if (sender == null)
            {
                throw new Exception("Logged-in user not found");
            }

            if (sender.Role == "Voter" && messageDto.To == null)
            {
                throw new Exception("Voter cannot send messages to everyone");
            }

            var message = _mesageMapper.MapMessageRequestDtoToMessage(messageDto);
            message.From = sender.UserId;
            message = await _messageRepository.Add(message);

            if (messageDto.To != null)
            {
                var recipient = allUsers.SingleOrDefault(u => u.UserId == messageDto.To);
                if (recipient == null)
                {
                    throw new Exception("Recipient not found");
                }

                if (sender.Role == "Voter" && recipient.Role != "Moderator")
                {
                    throw new Exception("Voter can only send messages to a Moderator");
                }

                var userMessage = new UserMessage
                {
                    UserId = recipient.UserId,
                    MessageId = message.Id
                };
                await _userMessageRepository.Add(userMessage);
            }
            else
            {
                var voters = await _voterRepository.GetAll();
                foreach (var voter in voters)
                {
                    var userMessage = new UserMessage
                    {
                        UserId = voter.Id,
                        MessageId = message.Id
                    };
                    await _userMessageRepository.Add(userMessage);
                }
            }
            return message;
        }

        public async Task<Message> DeleteMessage(Guid messageId)
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (email == null)
            {
                throw new Exception("No user logged in");
            }

            var allUsers = await _userRepository.GetAll();
            var currentUser = allUsers.SingleOrDefault(u => u.Username == email);
            if (currentUser == null)
            {
                throw new Exception("User not found");
            }

            var message = await _messageRepository.Get(messageId);
            if (message == null)
            {
                throw new Exception("Message not found");
            }

            if (message.From != currentUser.UserId)
            {
                throw new Exception("You can only delete your own message");
            }

            var userMessages = (await _userMessageRepository.GetAll())
                    .Where(um => um.MessageId == messageId)
                    .ToList();
            foreach (var userMsg in userMessages)
            {
                await _userMessageRepository.Delete(userMsg.Id);
            }
            return await _messageRepository.Delete(message.Id);
        }

        public async Task<UserMessage> ClearUserMessage(Guid messageId)
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing UserId in token");
            }

            var userMessages = await _userMessageRepository.GetAll();
            var msg = userMessages.SingleOrDefault(um => um.UserId == userId && um.MessageId == messageId);
            if (msg == null)
            {
                throw new Exception("Message not found");
            }
            return await _userMessageRepository.Delete(msg.Id);
        }

        public async Task ClearAllMessages()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing UserId in token");
            }

            var allUserMessages = await _userMessageRepository.GetAll();
            var messsagesToDelete = allUserMessages.Where(um => um.UserId == userId).ToList();
            foreach (var um in messsagesToDelete)
            {
                await _userMessageRepository.Delete(um.Id);
            }
        }
        public async Task<List<Message>> GetMessageForVoter()
        {
            var userIdClaim = _httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid or missing UserId in token");
            }

            var userMessages = await _userMessageRepository.GetAll();
            var userMessageIds = userMessages
                                    .Where(um => um.UserId == userId)
                                    .Select(um => um.MessageId)
                                    .ToList();

            var allMessages = await _messageRepository.GetAll();
            var messages = allMessages
                .Where(m => userMessageIds.Contains(m.Id))
                .OrderByDescending(m => m.SentAt)
                .ToList();

            return messages;
        }
        
        public async Task<List<Message>> GetMessagesForModerator()
        {
            var email = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (email == null) throw new Exception("No user logged in");

            var users = await _userRepository.GetAll();
            var user = users.SingleOrDefault(u => u.Username == email);
            if (user == null) throw new Exception("User not found");


            var allMessages = await _messageRepository.GetAll();
            var messages = allMessages
                                .Where(m => m.From == user.UserId)
                                .OrderByDescending(m => m.SentAt)
                                .ToList();
            return messages;
        }

        public async Task CleanupOldUserMessagesAsync()
        {
            int expiryDays = _configuration.GetValue<int>("MessageCleanupDays");

            var cutoffDate = DateTime.UtcNow.AddDays(-expiryDays);

            var allUserMessages = await _userMessageRepository.GetAll();
            var expiredMessages = allUserMessages
                .Where(um => um.CreatedAt < cutoffDate)
                .ToList();

            foreach (var userMessage in expiredMessages)
            {
                await _userMessageRepository.Delete(userMessage.Id);
            }
        }
    }
}
