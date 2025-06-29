namespace TrueVote.Models.DTOs
{
    public class PollResponseDto
    {
        public Poll? Poll { get; set; }
        public List<PollOption>? PollOptions { get; set; }
        public DateTime? VoteTime { get; set; }
    }
}