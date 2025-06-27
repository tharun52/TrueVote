using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class Poll
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        [EmailAddress]
        public string CreatedByEmail { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public bool IsDeleted { get; set; } = false;

        public Guid? PoleFileId { get; set; } = null;

        public ICollection<PollOption>? PollOptions { get; set; }
    }
}