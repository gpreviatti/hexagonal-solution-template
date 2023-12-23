using Hexagonal.Solution.Template.Domain.Common;
using System.Linq.Expressions;

namespace Hexagonal.Solution.Template.Application.Common.Repositories;
public interface IBaseRepository<TEntity> where TEntity : DomainEntity
{
    Task<TEntity> Add(TEntity entity);
    Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate);
    Task AddRange(TEntity[] entities);
    void BeginTransaction();
    Task<bool> CheckExistsByWhere(Expression<Func<TEntity, bool>> predicate);
    Task<bool> CheckExistsByWhereAsNoTracking(Expression<Func<TEntity, bool>> predicate);
    void CommitTransaction();
    Task<TEntity?> FirstOrDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate);
    Task<TEntity?> GetById(object id);
    Task<IList<TEntity>> GetByWhere(Expression<Func<TEntity, bool>> predicate);
    Task<IList<TEntity>> GetByWhereAsNoTracking(Expression<Func<TEntity, bool>> predicate);
    void Remove(TEntity entity);
    void RemoveRange(TEntity[] entities);
    void RollbackTransaction();
    void Update(TEntity entity);
}