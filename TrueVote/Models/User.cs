
using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class User
    {
        [Key]
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [Required]
        public string HashKey { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;

        [Required]
        public Guid UserId { get; set; }
    }
}