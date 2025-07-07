namespace TrueVote.Models.DTOs
{
    public class MessageRequestDto
    {
        public string Msg { get; set; } = string.Empty;

        public Guid? PollId { get; set; }

        public Guid? To { get; set; }
    }
}