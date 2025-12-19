using System.Linq.Expressions;
using Domain.Common;

namespace Application.Common.Repositories;
public interface IBaseRepository<TEntity> where TEntity : DomainEntity
{
    Task<int> AddAsync(TEntity entity, Guid correlationId, CancellationToken cancellationToken);
    Task<int> AddRangeAsync(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken);
    Task<int> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken);
    Task<int> UpdateAsync(TEntity entity, Guid correlationId, CancellationToken cancellationToken);
    Task<int> RemoveAsync(TEntity entity, Guid correlationId, CancellationToken cancellationToken);
    Task<int> RemoveRangeAsync(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken);

    Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken);
    Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken);

    Task<TEntity> FirstOrDefaultAsNoTrackingAsync(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    );
    Task<TResult> FirstOrDefaultAsNoTrackingAsync<TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken
    );
    Task<TEntity> GetByIdAsNoTrackingAsync(
        int id,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    );
    Task<TResult> GetByIdAsNoTrackingAsync<TResult>(
        int id,
        Guid correlationId,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken
    );
    Task<IList<TEntity>> GetByWhereAsync(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    );
    Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    );
    Task<IList<TResult>> GetByWhereAsNoTrackingAsync<TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken
    );
    Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync(
        Guid correlationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        params Expression<Func<TEntity, object>>[]? includes
    );

    Task<(IEnumerable<TResult> Items, int TotalRecords)> GetAllPaginatedAsync<TResult>(
        Guid correlationId,
        int page,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        Expression<Func<TEntity, bool>> predicate = null!
    );

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
