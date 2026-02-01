using System.Linq.Expressions;

namespace Infrastructure.DataAccess.Repositories;

public interface IGenericRepository<T>
    where T : class
{
    public Task<T?> GetAsync(Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includeProperties);


    public Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>>? predicate = null,
        params Expression<Func<T, object>>[] includeProperties);

    public Task AddAsync(T Entity);
    public Task AddBulkAsync(IEnumerable<T> Entities);
    public Task UpdateAsync(T Entity);

    public Task DeleteAsync(T Entity);
    public Task DeleteAllAsync(Expression<Func<T, bool>>? predicate = null);
}