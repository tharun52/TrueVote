using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class AuditRepository : Repository<Guid, AuditLog>
    {
        public AuditRepository(AppDbContext appDbContext) : base(appDbContext)
        {}
        public override async Task<AuditLog?> Get(Guid key)
        {
            return await _appDbContext.AuditLogs
                            .SingleOrDefaultAsync(a => a.AuditId == key)
                            ?? throw new Exception("No audit found with the given id");
                               
        }

        public override async Task<IEnumerable<AuditLog>> GetAll()
        {
            return await _appDbContext.AuditLogs.ToListAsync();
        }
    }
}