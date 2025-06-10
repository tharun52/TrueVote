using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class UserRepository : Repository<string, User>
    {
        public UserRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
        public override async Task<User?> Get(string key)
        {
            return await _appDbContext.Users
                .SingleOrDefaultAsync(u => u.Username == key);
        }

        public override async Task<IEnumerable<User>> GetAll()
        {
            return await _appDbContext.Users.ToListAsync();
        }
    }
}