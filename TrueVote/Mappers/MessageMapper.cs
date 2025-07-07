using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Mappers
{
    public class MessageMapper
    {
        public Message MapMessageRequestDtoToMessage(MessageRequestDto dto)
        {
            if (dto == null) throw new Exception("Give full dto");
            if (dto.To == null)
            {
                return new Message
                {
                    Msg = dto.Msg,
                    PollId = dto.PollId,
                    To = dto.To,
                    SentAt = DateTime.UtcNow
                };

            }
            else
            {                
                return new Message
                {
                    Msg = dto.Msg,
                    PollId = dto.PollId,
                    To = dto.To,
                    SentAt = DateTime.UtcNow
                };
            }
        }    
    }
}