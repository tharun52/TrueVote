using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class PollVoteRepository : Repository<Guid, PollVote>
    {
        public PollVoteRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }

        public override async Task<PollVote?> Get(Guid key)
        {
            return await _appDbContext.PollVotes
                                      .SingleOrDefaultAsync(pv => pv.Id == key);
        }

        public override async Task<IEnumerable<PollVote>> GetAll()
        {
            return await _appDbContext.PollVotes.ToListAsync();
        }
    }
}