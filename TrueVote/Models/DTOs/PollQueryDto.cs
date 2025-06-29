namespace TrueVote.Models.DTOs
{
    public class PollQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
        public string? CreatedByEmail { get; set; }
        public Guid? VoterId { get; set; }
        public bool ForVoting { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}