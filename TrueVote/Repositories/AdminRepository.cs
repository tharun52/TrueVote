using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class AdminRepository : Repository<Guid, Admin>
    {
        public AdminRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
        public override async Task<Admin> Get(Guid key)
        {
            return await _appDbContext.Admins
                .SingleOrDefaultAsync(a => a.Id == key)
                ?? throw new KeyNotFoundException($"Admin with ID {key} not found.");
        }
        public override async Task<IEnumerable<Admin>> GetAll()
        {
            return await _appDbContext.Admins.ToListAsync();
        }
    }
}