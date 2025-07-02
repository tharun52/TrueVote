using TrueVote.Interfaces;
using TrueVote.Models;
using TrueVote.Models.DTOs;

namespace TrueVote.Service
{
    public class AuditService : IAuditService
    {
        private readonly IRepository<Guid, AuditLog> _auditRepository;

        public AuditService(IRepository<Guid, AuditLog> auditRepository)
        {
            _auditRepository = auditRepository;
        }

        public async Task<PagedResponseDto<AuditLog>> GetAuditLogsPaged(AuditLogQueryDto query)
        {
            var auditLogs = (await _auditRepository.GetAll()).ToList();

            // Pagination
            int totalRecords = auditLogs.Count;
            int page = query.Page;
            int pageSize = query.PageSize;
            int totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            int skip = (page - 1) * pageSize;
            auditLogs = auditLogs.Skip(skip).Take(pageSize).ToList();

            return new PagedResponseDto<AuditLog>
            {
                Data = auditLogs,
                Pagination = new PaginationDto
                {
                    TotalRecords = totalRecords,
                    Page = page,
                    PageSize = pageSize,
                    TotalPages = totalPages
                }
            };
        }


        public async Task LogAsync(string description, Guid entityId, string? createdBy = null, string? updatedBy = null)
        {
            var auditLog = new AuditLog
            {
                AuditId = Guid.NewGuid(),
                Description = description,
                EntityId = entityId,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow,
                UpdatedBy = updatedBy,
                UpdatedAt = updatedBy != null ? DateTime.UtcNow : null
            };

            await _auditRepository.Add(auditLog);
        }
    }
}
