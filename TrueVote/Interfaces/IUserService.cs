namespace TrueVote.Interfaces
{
    public interface IUserService
    {
        public Task<object> GetUserDetailsByIdAsync(string userId);
    }
}