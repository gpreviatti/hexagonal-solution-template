using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Domain.Common;

namespace Application.Common.Repositories;
public interface IBaseRepository
{
    Task<int> AddAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity;
    Task<int> AddRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity;
    Task<int> UpdateAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity;
    Task<int> RemoveAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity;
    Task<int> RemoveRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity;
    IQueryable<TEntity> GetQueryable<TEntity>(Guid correlationId, bool? newContext = null, [CallerMemberName] string methodName = null!) where TEntity : DomainEntity;
    Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity>(
        Guid correlationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity;

    Task<(IEnumerable<TResult> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity, TResult>(
        Guid correlationId,
        int page,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        string? sortBy = null,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null,
        Expression<Func<TEntity, bool>> predicate = null!,
        bool? newContext = null
    ) where TEntity : DomainEntity;
}
