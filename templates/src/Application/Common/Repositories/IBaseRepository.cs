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

    Task<TEntity> FirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    );
    Task<TEntity> GetByIdAsNoTrackingAsync(
        int id,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    );
    Task<IList<TEntity>> GetByWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    );
    Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    );
    Task<(IList<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string sortBy = null,
        bool sortDescending = false,
        string searchPropertyName = null,
        string searchValue = null,
        params Expression<Func<TEntity, object>>[] includes
    );

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
