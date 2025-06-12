using TrueVote.Interfaces;
using TrueVote.Models;

namespace TrueVote.Service
{
    public class AuditService : IAuditService
    {
        private readonly IRepository<Guid, AuditLog> _auditRepository;

        public AuditService(IRepository<Guid, AuditLog> auditRepository)
        {
            _auditRepository = auditRepository;
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
