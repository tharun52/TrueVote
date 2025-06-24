using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class VoterEmail
    {
        [Key]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public Guid ModeratorId { get; set; }

        [Required]
        public Boolean IsUsed { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
    }
}
