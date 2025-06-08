

using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class PollVote
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PollOptionId { get; set; } 

        [Required]
        public DateTime Timestamp { get; set; }

    }
}