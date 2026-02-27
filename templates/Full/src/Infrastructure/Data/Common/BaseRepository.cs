using System.Diagnostics;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Application.Common.Helpers;
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
    private readonly Stopwatch _stopwatch = new();
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory = dbContextFactory;
    private readonly MyDbContext _dbContext = dbContextFactory.CreateDbContext();
    private readonly string _className = nameof(BaseRepository);

    private async Task<TResult> HandleBaseQueryAsync<TEntity, TResult>(
        Func<DbSet<TEntity>, Task<TResult>> query,
        Guid correlationId,
        bool? newContext = false,
        [CallerMemberName]
        string methodName = null!
    ) where TEntity : DomainEntity
    {
        _stopwatch.Restart();

        Logs.DebugStartingOperation(logger, _className, methodName, correlationId);

        var dbSet = _dbContext.Set<TEntity>();
        if (newContext.GetValueOrDefault())
            dbSet = _dbContextFactory.CreateDbContext().Set<TEntity>();

        var result = await query.Invoke(dbSet);

        Logs.DebugFinishedOperation(logger, _className, methodName, correlationId, _stopwatch.ElapsedMilliseconds);

        return result;
    }

    public IQueryable<TEntity> GetQueryable<TEntity>(
        Guid correlationId,
        bool? newContext = null,
        [CallerMemberName]
        string methodName = null!
    ) where TEntity : DomainEntity
    {
        _stopwatch.Restart();

        Logs.DebugStartingOperation(logger, _className, methodName, correlationId);

        var dbSet = _dbContext.Set<TEntity>();
        if (newContext.GetValueOrDefault())
            dbSet = _dbContextFactory.CreateDbContext().Set<TEntity>();

        Logs.DebugFinishedOperation(logger, _className, methodName, correlationId, _stopwatch.ElapsedMilliseconds);

        return dbSet;
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
                    EF.Functions.ILike(EF.Property<string>(e, searchByValue.Key), $"%{searchByValue.Value}%")
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
                    EF.Functions.ILike(EF.Property<string>(e, searchByValue.Key), $"%{searchByValue.Value}%")
                );

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }, correlationId, newContext);
}
