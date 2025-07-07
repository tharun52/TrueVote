using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IMessageService
    {
        public Task<Message> AddMessage(MessageRequestDto messageDto);
        public Task<Message> DeleteMessage(Guid messageId);
        public Task CleanupOldUserMessagesAsync();
        public Task ClearAllMessages();
        public Task<UserMessage> ClearUserMessage(Guid messageId);
        public Task<List<Message>> GetMessageForVoter();
        public Task<List<Message>> GetMessagesForModerator();
    }
}