using System.Linq.Expressions;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Common;
public interface IBaseRepository<T> where T : class, new()
{
    Task<T> Add(T entity);
    Task<T> AddOrUpdateIfNotExistsAsync(T entity, Expression<Func<T, bool>> predicate);
    Task AddRange(T[] entities);
    void BeginTransaction();
    Task<bool> CheckExistsByWhere(Expression<Func<T, bool>> predicate);
    Task<bool> CheckExistsByWhereAsNoTracking(Expression<Func<T, bool>> predicate);
    void CommitTransaction();
    Task<T?> FirstOrDefaultAsNoTracking(Expression<Func<T, bool>> predicate);
    Task<T?> GetById(object id);
    Task<IList<T>> GetByWhere(Expression<Func<T, bool>> predicate);
    Task<IList<T>> GetByWhereAsNoTracking(Expression<Func<T, bool>> predicate);
    void Remove(T entity);
    void RemoveRange(T[] entities);
    void RollbackTransaction();
    void Update(T entity);
}