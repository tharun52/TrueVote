using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class VoterRepository : Repository<Guid, Voter>
    {
        public VoterRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
        public override async Task<Voter> Get(Guid key)
        {
            return await _appDbContext.Voters
                .SingleOrDefaultAsync(v => v.Id == key)
                ?? throw new KeyNotFoundException($"Voter with ID {key} not found.");
        }

        public override async Task<IEnumerable<Voter>> GetAll()
        {
            return await _appDbContext.Voters.ToListAsync();
        }
    }
}