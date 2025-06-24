using Microsoft.EntityFrameworkCore;
using TrueVote.Contexts;
using TrueVote.Models;

namespace TrueVote.Repositories
{
    public class VoterEmailRepository : Repository<string, VoterEmail>
    {
        public VoterEmailRepository(AppDbContext appDbContext) : base(appDbContext)
        {
        }
        public override async Task<VoterEmail?> Get(string key)
        {
            return await _appDbContext.VoterEmails.SingleOrDefaultAsync(ve => ve.Email == key);
        }

        public override async Task<IEnumerable<VoterEmail>> GetAll()
        {
            return await _appDbContext.VoterEmails.ToListAsync();
        }
    }
}