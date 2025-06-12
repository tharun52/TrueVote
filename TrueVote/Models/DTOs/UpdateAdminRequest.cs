namespace TrueVote.Models.DTOs
{
    public class UpdateAdminRequest
    {
        public string? PrevPassword { get; set; }
        public string? NewPassword { get; set; }
        public string? Name { get; set; }
    }
}