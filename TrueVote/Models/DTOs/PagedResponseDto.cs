namespace TrueVote.Models.DTOs
{
    public class PagedResponseDto<T>
    {
        public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
        public PaginationDto Pagination { get; set; } = new PaginationDto();
    }

    public class PaginationDto
    {
        public int TotalRecords { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
}