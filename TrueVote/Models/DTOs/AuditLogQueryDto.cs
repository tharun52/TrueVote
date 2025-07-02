namespace TrueVote.Models.DTOs
{
    public class AuditLogQueryDto
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
    }

}