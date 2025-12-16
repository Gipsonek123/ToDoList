using WebApp.Models.EntityFramework;
using WebApp.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace WebApp.Services.Implementations
{
    public class GenericDataService<T> : IGenericDataService<T> where T : class
    {
        private readonly WebAppDbContext _dbContext;

        public GenericDataService(WebAppDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<T> Create(T entity)
        {
            _dbContext.Set<T>().Add(entity);
            await _dbContext.SaveChangesAsync();
            return entity;
        }

        public async Task Delete(int id)
        {
            var existing = await _dbContext.Set<T>().FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Don't found object {typeof(T).Name} of Id {id}.");
            }
            _dbContext.Set<T>().Remove(existing);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAll()
        {
            return await _dbContext.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T> GetById(int id)
        {
            var entity = await _dbContext.Set<T>().FindAsync(id);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Don't found object {typeof(T).Name} of Id {id}.");
            }
            return entity;
        }

        public async Task<T> GetByUsername(string username)
        {
            var entity = await _dbContext.Set<T>().FirstOrDefaultAsync(e => EF.Property<string>(e, "Username") == username);
            return entity;
        }

        public async Task<T> Update(int id, T entity)
        {
            var existing = await _dbContext.Set<T>().FindAsync(id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Don't found object {typeof(T).Name} of Id {id}.");
            }
            _dbContext.Entry(existing).CurrentValues.SetValues(entity);
            await _dbContext.SaveChangesAsync();
            return existing;
        }
    }
}
