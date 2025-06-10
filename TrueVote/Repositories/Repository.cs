using TrueVote.Contexts;
using TrueVote.Interfaces;

namespace TrueVote.Repositories
{
    public abstract class Repository<K, T> : IRepository<K, T> where T : class
    {
        public readonly AppDbContext _appDbContext;

        public Repository(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public async Task<T> Add(T item)
        {
            _appDbContext.Add(item);
            await _appDbContext.SaveChangesAsync();
            return item;
        }

        public async Task<T> Delete(K key)
        {
            var item = await Get(key);
            if (item != null)
            {
                _appDbContext.Remove(item);
                await _appDbContext.SaveChangesAsync();
                return item;
            }
            else
            {
                throw new Exception("Element not found for deletion");
            }
        }

        public abstract Task<T?> Get(K key);

        public abstract Task<IEnumerable<T>> GetAll();

        public async Task<T> Update(K key, T item)
        {
            var updateItem = await Get(key);
            if (updateItem != null)
            {
                _appDbContext.Entry(updateItem).CurrentValues.SetValues(updateItem);
                await _appDbContext.SaveChangesAsync();
                return updateItem;
            }
            else
            {
                throw new Exception("Element not found for deletion");
            }
        }
    }
}