using System.Linq.Expressions;
using Domain.Common;

namespace Application.Common.Repositories;
public interface IBaseRepository<TEntity> where TEntity : DomainEntity
{
    Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task<int> AddRangeAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    int Update(TEntity entity);

    Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken);
    Task<int> RemoveRangeAsync(TEntity[] entities, CancellationToken cancellationToken);

    Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task<TEntity> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity> GetByIdAsNoTrackingAsync(object id, CancellationToken cancellationToken);
    Task<IList<TEntity>> GetByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
