

using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class ModeratorRepository : Repository<Guid, Moderator>
    {
        public ModeratorRepository(AppDbContext appDbContext) : base(appDbContext)
        {

        }

        public override async Task<Moderator> Get(Guid key)
        {
            var moderator = await _appDbContext.Moderators
                .Include(c => c.Polls)
                .FirstOrDefaultAsync(c => c.Id == key);

            return moderator ?? throw new KeyNotFoundException($"Moderators with ID {key} not found.");
        }

        public override async Task<IEnumerable<Moderator>> GetAll()
        {
            var moderators = _appDbContext.Moderators
                .Where(c => !c.IsDeleted)
                .Include(c => c.Polls);
            if (!moderators.Any())
            {
                throw new Exception("No Moderators found");
            }
            return await moderators.ToListAsync();
        }
    }
}