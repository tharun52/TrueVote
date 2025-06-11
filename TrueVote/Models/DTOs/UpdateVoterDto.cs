namespace TrueVote.Models.DTOs
{
    public class UpdateVoterDto
    {
        public string? Name { get; set; }
        public int? Age { get; set; }
        public bool? IsDeleted { get; set; }
        public string? PrevPassword { get; set; }
        public string? NewPassword { get; set; }
    }
}