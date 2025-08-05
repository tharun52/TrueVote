using System.ComponentModel.DataAnnotations;

namespace TrueVote.Models
{
    public class MagicLoginToken
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
    }
}