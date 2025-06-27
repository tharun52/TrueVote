using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class PollFileRepository : Repository<Guid, PollFile>
    {
        public PollFileRepository(AppDbContext appDbContext) : base(appDbContext)
        {}

        public override async Task<PollFile?> Get(Guid key)
        {
            return await _appDbContext.PollFiles.SingleOrDefaultAsync(pf => pf.Id == key)
                ?? throw new Exception("File not found");
        }

        public override async Task<IEnumerable<PollFile>> GetAll()
        {
            return await _appDbContext.PollFiles.ToListAsync();
        }
    }
}