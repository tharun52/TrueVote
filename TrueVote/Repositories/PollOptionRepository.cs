using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class PollOptionRepository: Repository<Guid, PollOption>
    {
        public PollOptionRepository(AppDbContext appDbContext) : base(appDbContext)
        { }

        public override async Task<PollOption> Get(Guid key)
        {
            return await _appDbContext.PollOptions
                    .SingleOrDefaultAsync(po => po.Id == key) ?? new PollOption();
        }

        public override async Task<IEnumerable<PollOption>> GetAll()
        {
            return await _appDbContext.PollOptions.ToListAsync();
        }
    }
}