
namespace TrueVote.Models.DTOs
{
    public class AddAdminRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string SeceretAdminKey { get; set; } = string.Empty;
    }
}