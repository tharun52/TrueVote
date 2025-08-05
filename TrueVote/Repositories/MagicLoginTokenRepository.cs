using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class MagicLoginTokenRepository : Repository<Guid, MagicLoginToken>
    {
        public MagicLoginTokenRepository(AppDbContext appDbContext) : base(appDbContext)
        {}
        public override async Task<MagicLoginToken?> Get(Guid key)
        {
            return await _appDbContext.MagicLoginTokens.SingleOrDefaultAsync(mt => mt.Id == key);
        }

        public override async Task<IEnumerable<MagicLoginToken>> GetAll()
        {
            return await _appDbContext.MagicLoginTokens.ToListAsync();
        }
    }
}