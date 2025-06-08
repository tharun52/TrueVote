

using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class Voter
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int Age { get; set; } = 0;

        [Required]
        public bool IsDeleted{ get; set; } = false;

    }
}