using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class Message
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Msg { get; set; } = string.Empty;

        public Guid From { get; set; }
        
        public Guid? PollId { get; set; }

        public Guid? To { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }

    public class UserMessage
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid MessageId { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}