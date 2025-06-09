using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class PollRepository : Repository<Guid, Poll>
    {
        public PollRepository(AppDbContext appDbContext) : base(appDbContext)
        {}
        public override async Task<Poll> Get(Guid key)
        {
            return await _appDbContext.Polls
                    .SingleOrDefaultAsync(p => p.Id == key)
                    ?? new Poll();
        }

        public override async Task<IEnumerable<Poll>> GetAll()
        {
            return await _appDbContext.Polls.ToListAsync();
        }
    }
}