namespace TrueVote.Interfaces
{
    public interface IAuditService
    {
        public Task LogAsync(string description, Guid entityId, string? createdBy = null, string? updatedBy = null);
    }
}