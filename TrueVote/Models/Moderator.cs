using System.ComponentModel.DataAnnotations;


namespace TrueVote.Models
{

    public class Moderator
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;

        public ICollection<Poll>? Polls { get; set; }
    }
}