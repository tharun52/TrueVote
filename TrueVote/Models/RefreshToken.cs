using System.ComponentModel.DataAnnotations;


namespace TrueVote.Models
{
    public class RefreshToken
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty; 

        [Required]
        public string Token { get; set; } = string.Empty;     

        [Required]
        public DateTime ExpiresAt { get; set; }               

        public bool IsRevoked { get; set; } = false;   
    }
}