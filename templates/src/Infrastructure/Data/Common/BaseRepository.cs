using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Common;

public class BaseRepository(ILogger<BaseRepository> logger, MyDbContext dbContext) : IBaseRepository
{
    protected readonly ILogger<BaseRepository> logger = logger;
    protected readonly MyDbContext dbContext = dbContext;
    private readonly Stopwatch _stopwatch = new();

    private async Task<TResult> HandleBaseQueryAsync<TEntity, TResult>(
        Func<DbSet<TEntity>, Task<TResult>> query,
        Guid correlationId,
        [CallerMemberName]
        string methodName = null!
    ) where TEntity : DomainEntity
    {
        _stopwatch.Restart();

        logger.LogDebug(
            "[BaseRepository] | [{Method}] | CorrelationId: {CorrelationId} | Starting database operation.",
            methodName,
            correlationId
        );

        var dbEntitySet = dbContext.Set<TEntity>();

        var result = await query.Invoke(dbEntitySet);

        logger.LogDebug(
            "[BaseRepository] | [{Method}] | CorrelationId: {CorrelationId} | Query executed in {ElapsedMilliseconds} ms.",
            methodName,
            correlationId,
            _stopwatch.ElapsedMilliseconds
        );

        return result;
    }

    public async Task<int> AddAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity => 
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        await dbEntitySet.AddAsync(entity, cancellationToken);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<int> AddRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        await dbEntitySet.AddRangeAsync(entities, cancellationToken);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<int> AddOrUpdateIfNotExistsAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity, cancellationToken);
        else
            dbEntitySet.Update(entity);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<int> UpdateAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        var updatedEntity = dbEntitySet.Update(entity);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<int> RemoveAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        dbEntitySet.Remove(entity);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<int> RemoveRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        dbEntitySet.RemoveRange(entities);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId);

    public async Task<bool> CheckExistsByWhereAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, bool>(async dbEntitySet =>
    {
        return await dbEntitySet.AnyAsync(predicate, cancellationToken);
    }, correlationId);

    public async Task<bool> CheckExistsByWhereAsNoTrackingAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, bool>(async dbEntitySet =>
    {
        return await dbEntitySet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }, correlationId);

    public async Task<TEntity> GetByIdAsNoTrackingAsync<TEntity>(
        int id,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    ) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, TEntity>(async dbEntitySet =>
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken) ?? default!;
    }, correlationId);

    public async Task<IList<TEntity>> GetByWhereAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    ) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, IList<TEntity>>(async dbEntitySet =>
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }, correlationId);

    public async Task<IList<TEntity>> GetByWhereAsNoTrackingAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    ) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, IList<TEntity>>(async dbEntitySet =>
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }, correlationId);

    public async Task<TEntity> FirstOrDefaultAsNoTrackingAsync<TEntity>(
        Expression<Func<TEntity, bool>> predicate,
        Guid correlationId,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    ) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, TEntity>(async dbEntitySet =>
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken) ?? default!;
    }, correlationId);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken) 
    {
        if (dbContext.Database.CurrentTransaction == null)
            await dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction != null)
        {
            dbContext.SaveChanges();
            await dbContext.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction != null)
            await dbContext.Database.CurrentTransaction.RollbackAsync(cancellationToken);
    }

    public async Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity>(
        int page,
        int pageSize,
        Guid correlationId,
        CancellationToken cancellationToken,
        string? sortBy = null!,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null!,
        params Expression<Func<TEntity, object>>[] includes
    ) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, (IEnumerable<TEntity> Items, int TotalRecords)>(async dbEntitySet =>
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();
        foreach (var include in includes)
        {
            query = query.Include(include);
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }
        else
        {
            query = query.OrderBy(e => e.CreatedAt);
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        if (searchByValues != null && searchByValues.Count != 0)
        {
            foreach (var searchByValue in searchByValues)
            {
                query = query.Where(e =>
                    EF.Property<string>(e, searchByValue.Key).Contains(searchByValue.Value.ToLowerInvariant())
                );
            }
        }

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }, correlationId);

    private static IQueryable<TEntity> SetIncludes<TEntity>(
        Expression<Func<TEntity, object>>[] includes,
        IQueryable<TEntity> query
    ) where TEntity : DomainEntity
    {
        if (includes != null && includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        return query;
    }
}
