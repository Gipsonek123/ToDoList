namespace WebApp.Services.Interfaces
{
    public interface IGenericDataService<T>
    {
        Task<T> Create(T entity);
        Task<T> GetById(int id);
        Task<T> GetByUsername(string username);
        Task<IEnumerable<T>> GetAll();
        Task<T> Update(int id, T entity);
        Task Delete(int id);
    }
}
