namespace TrueVote.Models.DTOs
{
    public class AddVoterRequestDto
    {
        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public int Age { get; set; } = 0;
    }
}