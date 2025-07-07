using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class UserMesssageRepository : Repository<Guid, UserMessage>
    {
        public UserMesssageRepository(AppDbContext appDbContext) : base(appDbContext)
        {}
        public override async Task<UserMessage?> Get(Guid key)
        {
            return await _appDbContext.UserMessages
                            .SingleOrDefaultAsync(a => a.Id == key)
                            ?? throw new Exception("No User Message found with the given id");
                               
        }
        public override async Task<IEnumerable<UserMessage>> GetAll()
        {
            return await _appDbContext.UserMessages.ToListAsync();
        }
    }
}