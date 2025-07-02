using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Interfaces
{
    public interface IAuditService
    {
        public  Task<PagedResponseDto<AuditLog>> GetAuditLogsPaged(AuditLogQueryDto query);
        public Task LogAsync(string description, Guid entityId, string? createdBy = null, string? updatedBy = null);
    }
}