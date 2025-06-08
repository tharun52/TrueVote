using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class VoterPoll
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public Guid VoterId { get; set; }

        [Required]
        public Guid PollId { get; set; }

        public bool HasVoted { get; set; } = false;

        public DateTime? VotedAt { get; set; }
    }
}