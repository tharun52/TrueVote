namespace TrueVote.Models.DTOs
{
    public class ModeratorQueryDto
    {
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDesc { get; set; } = false;
        public string? Email { get; set; }
        public bool? IsDeleted { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}