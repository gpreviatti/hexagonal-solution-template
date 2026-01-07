using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Common;

public class BaseRepository(
    ILogger<BaseRepository> logger,
    IDbContextFactory<MyDbContext> dbContextFactory
) : IBaseRepository
{
    protected readonly ILogger<BaseRepository> logger = logger;
    private readonly Stopwatch _stopwatch = new();
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory = dbContextFactory;
    private readonly MyDbContext _dbContext = dbContextFactory.CreateDbContext();

    private async Task<TResult> HandleBaseQueryAsync<TEntity, TResult>(
        Func<DbSet<TEntity>, Task<TResult>> query,
        Guid correlationId,
        bool? newContext = false,
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

        var dbSet = _dbContext.Set<TEntity>();
        if (newContext.GetValueOrDefault())
            dbSet = _dbContextFactory.CreateDbContext().Set<TEntity>();

        var result = await query.Invoke(dbSet);

        logger.LogDebug(
            "[BaseRepository] | [{Method}] | CorrelationId: {CorrelationId} | Query executed in {ElapsedMilliseconds} ms.",
            methodName,
            correlationId,
            _stopwatch.ElapsedMilliseconds
        );

        return result;
    }

    public async Task<int> AddAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        await dbEntitySet.AddAsync(entity, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<int> AddRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        await dbEntitySet.AddRangeAsync(entities, cancellationToken);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<int> AddOrUpdateIfNotExistsAsync<TEntity>(TEntity entity, Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        if (!await dbEntitySet.AsNoTracking().AnyAsync(predicate))
            await dbEntitySet.AddAsync(entity, cancellationToken);
        else
            dbEntitySet.Update(entity);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<int> UpdateAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        var updatedEntity = dbEntitySet.Update(entity);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<int> RemoveAsync<TEntity>(TEntity entity, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        dbEntitySet.Remove(entity);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<int> RemoveRangeAsync<TEntity>(TEntity[] entities, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, int>(async dbEntitySet =>
    {
        dbEntitySet.RemoveRange(entities);

        return await _dbContext.SaveChangesAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<bool> CheckExistsByWhereAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, bool>(async dbEntitySet =>
    {
        return await dbEntitySet.AnyAsync(predicate, cancellationToken);
    }, correlationId, newContext);

    public async Task<bool> CheckExistsByWhereAsNoTrackingAsync<TEntity>(Expression<Func<TEntity, bool>> predicate, Guid correlationId, CancellationToken cancellationToken, bool? newContext = null) where TEntity : DomainEntity =>
    await HandleBaseQueryAsync<TEntity, bool>(async dbEntitySet =>
    {
        return await dbEntitySet.AsNoTracking().AnyAsync(predicate, cancellationToken);
    }, correlationId, newContext);
    public async Task<TEntity> GetByIdAsNoTrackingAsync<TEntity>(
        int id,
        Guid correlationId,
        CancellationToken cancellationToken,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, TEntity>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsNoTracking();

        if (includes is not null)
            query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken) ?? default!;
    }, correlationId, newContext);

    public async Task<TResult> GetByIdAsNoTrackingAsync<TEntity, TResult>(
        int id,
        Guid correlationId,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        bool? newContext = null
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, TResult>(async dbEntitySet => await dbEntitySet
        .Where(o => o.Id == id)
        .Select(selector)
        .FirstOrDefaultAsync(cancellationToken) ?? default!,
        correlationId,
        newContext
    );

    public async Task<IList<TEntity>> GetByWhereAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, IList<TEntity>>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsQueryable();

        if (includes is not null)
            query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<IList<TEntity>> GetByWhereAsNoTrackingAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, IList<TEntity>>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsNoTracking();

        if (includes is not null)
            query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }, correlationId, newContext);

    public async Task<IList<TResult>> GetByWhereAsNoTrackingAsync<TEntity, TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        bool? newContext = null
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, IList<TResult>>(async dbEntitySet =>
        await dbEntitySet.Where(predicate).Select(selector).ToListAsync(cancellationToken),
        correlationId,
        newContext
    );

    public async Task<TEntity> FirstOrDefaultAsNoTrackingAsync<TEntity>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, TEntity>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsNoTracking();

        if (includes is not null)
            query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken) ?? default!;
    }, correlationId, newContext);

    public async Task<TResult> FirstOrDefaultAsNoTrackingAsync<TEntity, TResult>(
        Guid correlationId,
        Expression<Func<TEntity, bool>> predicate,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        bool? newContext = null
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, TResult>(async dbEntitySet => await dbEntitySet
        .Where(predicate)
        .Select(selector)
        .FirstOrDefaultAsync(cancellationToken) ?? default!,
        correlationId,
        newContext
    );

    public async Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity>(
        Guid correlationId,
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string? sortBy = null!,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null!,
        bool? newContext = null,
        params Expression<Func<TEntity, object>>[]? includes
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, (IEnumerable<TEntity> Items, int TotalRecords)>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsNoTracking();

        if (includes is not null)
            foreach (var include in includes)
                query = query.Include(include);

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        else
            query = query.OrderBy(e => e.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);

        if (searchByValues != null && searchByValues.Count != 0)
            foreach (var searchByValue in searchByValues)
                query = query.Where(e =>
                    EF.Property<string>(e, searchByValue.Key).Contains(searchByValue.Value.ToLowerInvariant())
                );

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }, correlationId, newContext);

    public async Task<(IEnumerable<TResult> Items, int TotalRecords)> GetAllPaginatedAsync<TEntity, TResult>(
        Guid correlationId,
        int page,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        string? sortBy = null!,
        bool sortDescending = false,
        Dictionary<string, string>? searchByValues = null!,
        Expression<Func<TEntity, bool>> predicate = null!,
        bool? newContext = null
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, (IEnumerable<TResult> Items, int TotalRecords)>(async dbEntitySet =>
    {
        var query = dbEntitySet.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        else
            query = query.OrderBy(e => e.CreatedAt);

        var totalRecords = await query.CountAsync(cancellationToken);

        if (searchByValues != null && searchByValues.Count != 0)
            foreach (var searchByValue in searchByValues)
                query = query.Where(e =>
                    EF.Property<string>(e, searchByValue.Key).Contains(searchByValue.Value.ToLowerInvariant())
                );

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }, correlationId, newContext);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction == null)
            await _dbContext.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction != null)
        {
            _dbContext.SaveChanges();
            await _dbContext.Database.CurrentTransaction.CommitAsync(cancellationToken);
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken)
    {
        if (_dbContext.Database.CurrentTransaction != null)
            await _dbContext.Database.CurrentTransaction.RollbackAsync(cancellationToken);
    }

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
