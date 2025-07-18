namespace TrueVote.Models.DTOs
{
    public class UpdatePollRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public List<string>? OptionTexts { get; set; }
        public IFormFile? PollFile { get; set; }
        public bool? IsDeleted { get; set; }
        public bool? IsVoteCountVisible { get; set; } 
    }
}