namespace TrueVote.Models.DTOs
{
    public class UpdateVoterAsModeratorDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public bool? IsDeleted { get; set; }
    }
}