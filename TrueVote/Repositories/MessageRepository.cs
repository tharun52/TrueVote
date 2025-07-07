using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class MesssageRepository : Repository<Guid, Message>
    {
        public MesssageRepository(AppDbContext appDbContext) : base(appDbContext)
        {}
        public override async Task<Message?> Get(Guid key)
        {
            return await _appDbContext.Messages
                            .SingleOrDefaultAsync(a => a.Id == key)
                            ?? throw new Exception("No Message found with the given id");
                               
        }
        public override async Task<IEnumerable<Message>> GetAll()
        {
            return await _appDbContext.Messages.ToListAsync();
        }
    }
}