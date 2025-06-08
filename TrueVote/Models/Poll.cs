using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class Poll
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string CreatedByEmail { get; set; } = string.Empty;

        [Required]
        public DateOnly StartDate { get; set; }

        [Required]
        public DateOnly EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public PoleFile? PoleFile { get; set; }
        
        public ICollection<PollOption>? PollOptions { get; set; }
    }
}