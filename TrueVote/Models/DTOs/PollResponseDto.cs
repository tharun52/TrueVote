namespace TrueVote.Models.DTOs
{
    public class PollResponseDto
    {
        public Poll? Poll { get; set; }
        public List<PollOption>? PollOptions{ get; set; }
        public string? PollImageBase64 { get; set; }
        public string? PollImageType { get; set; } 
    }
}