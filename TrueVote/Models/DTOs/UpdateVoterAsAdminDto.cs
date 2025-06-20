namespace TrueVote.Models.DTOs
{
    public class UpdateVoterAsAdminDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public bool? IsDeleted { get; set; }
    }
}