using System.Linq.Expressions;
using Domain.Common;

namespace Application.Common.Repositories;
public interface IBaseRepository
{
    Task<int> AddAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;
    Task<int> AddRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;
    Task<int> AddOrUpdateIfNotExistsAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;

    Task<int> UpdateAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;
    Task<int> RemoveAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;
    Task<int> RemoveRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;

    Task<bool> CheckExistsByWhereAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;
    Task<bool> CheckExistsByWhereAsNoTrackingAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity;

    Task<TEntity> FirstOrDefaultAsNoTrackingAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<TResult> FirstOrDefaultAsNoTrackingAsync<TEntity, TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<TEntity> GetByIdAsNoTrackingAsync<TEntity>(
        int id,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<TResult> GetByIdAsNoTrackingAsync<TEntity, TResult>(
        int id,
        Guid correlationId,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<IList<TEntity>> GetByWhereAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<IList<TEntity>> GetByWhereAsNoTrackingAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<IList<TResult>> GetByWhereAsNoTrackingAsync<TEntity, TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;
    Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity>(
        int page,
        int pageSize,
        Guid correlationId,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;

    Task<(IEnumerable<TResult> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity, TResult>(
        int page,
        int pageSize,
        Guid correlationId,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        Expression<Func<TEntity, bool>> predicate = null!,
        Expression<Func<TEntity, TResult>> selector = null!,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}
