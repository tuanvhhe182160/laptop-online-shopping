using System.Linq.Expressions;

namespace WebAPI.Repositories
{
    public interface IGenericRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(object id);
        Task AddAsync(T entity);
        void Update(T entity);
        void Delete(T entity);
        Task SaveAsync();
        Task<T?> FindAsync(Expression<Func<T, bool>> predicate);
        IQueryable<T> GetQueryable();
    }
}
