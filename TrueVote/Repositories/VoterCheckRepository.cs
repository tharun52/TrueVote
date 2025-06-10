using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class VoterCheckRepository : Repository<Guid, VoterCheck>
    {
        public VoterCheckRepository(AppDbContext appDbContext) : base(appDbContext)
        { }

        public override async Task<VoterCheck?> Get(Guid key)
        {
            return await _appDbContext
                            .VoterChecks
                            .SingleOrDefaultAsync(vc => vc.Id == key);
            
        }

        public override async Task<IEnumerable<VoterCheck>> GetAll()
        {
            return await _appDbContext.VoterChecks.ToListAsync();
        }
    }
}