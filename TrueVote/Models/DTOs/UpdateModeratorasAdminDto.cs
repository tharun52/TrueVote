namespace TrueVote.Models.DTOs
{
    public class UpdateModeratorasAdminDto
    {
        public string Name { get; set; } = string.Empty;

        public bool IsDeleted { get; set; } = false;
    }
}