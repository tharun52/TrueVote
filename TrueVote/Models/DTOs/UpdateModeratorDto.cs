namespace TrueVote.Models.DTOs
{
    public class UpdateModeratorDto
    {
        public string Name { get; set; } = string.Empty;
        public string PrevPassword { get; set; } = string.Empty;
        public string? NewPassword { get; set; }
        public bool IsDeleted { get; set; }
    }
}