namespace TrueVote.Models.DTOs
{
    public class UserLoginResponse
    {
        public string Username { get; set; } = string.Empty;
        public string? Token { get; set; }
        public string? Role { get; set; }
    }
}