using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class PollOption
    {
         [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid PollId { get; set; } 

        [Required]
        public string OptionText { get; set; } = string.Empty;

        public int VoteCount { get; set; } = 0;
        
        public bool IsDeleted { get; set; } = false;
    }
}