namespace TrueVote.Models.DTOs
{
    public class AddPollRequestDto
    {
        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public DateOnly StartDate { get; set; }

        public DateOnly EndDate { get; set; }

        public List<string> OptionText { get; set; } = new List<string>();

        public IFormFile? PollFile { get; set; }
    }
}