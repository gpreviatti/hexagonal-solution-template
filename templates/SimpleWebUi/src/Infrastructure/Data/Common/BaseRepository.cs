using System.Linq.Expressions;
using Core.Common.Helpers;
using Core.Common.Repositories;
using Core.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Data.Common;

public class BaseRepository(
    ILogger<BaseRepository> logger,
    IDbContextFactory<MyDbContext> dbContextFactory
) : IBaseRepository
{
    private readonly IDbContextFactory<MyDbContext> _dbContextFactory = dbContextFactory;
    private readonly MyDbContext _dbContext = dbContextFactory.CreateDbContext();

    private async Task<TResult> HandleBaseQueryAsync<TEntity, TResult>(
        Func<DbSet<TEntity>, Task<TResult>> query,
        Guid correlationId,
        bool? newContext = false
    ) where TEntity : DomainEntity
    {
        Logs.DebugStartingOperation(logger, correlationId);

        var dbSet = _dbContext.Set<TEntity>();
        if (newContext.GetValueOrDefault())
            dbSet = _dbContextFactory.CreateDbContext().Set<TEntity>();

        var result = await query.Invoke(dbSet);

        Logs.DebugFinishedOperation(logger, correlationId);

        return result;
    }

    public IQueryable<TEntity> GetQueryable<TEntity>(
        Guid correlationId,
        bool? newContext = null
    ) where TEntity : DomainEntity
    {
        Logs.DebugStartingOperation(logger, correlationId);

        var dbSet = _dbContext.Set<TEntity>();
        if (newContext.GetValueOrDefault())
            dbSet = _dbContextFactory.CreateDbContext().Set<TEntity>();

        Logs.DebugFinishedOperation(logger, correlationId);

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
        var totalRecords = _dbContextFactory
            .CreateDbContext()
            .Set<TEntity>()
            .CountAsync(cancellationToken);

        var query = dbEntitySet.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        else
            query = query.OrderBy(e => e.CreatedAt);


        if (searchByValues != null && searchByValues.Count != 0)
            foreach (var searchByValue in searchByValues)
                query = query.Where(e =>
                    EF.Functions.ILike(EF.Property<string>(e, searchByValue.Key), $"%{searchByValue.Value}%")
                );

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalRecords, items);

        return (await items, await totalRecords);
    }, correlationId, newContext);

    public async Task<(IEnumerable<TResult> Items, int TotalRecords)> GetAllFullTextSearchPaginatedAsync<TEntity, TResult>(
        Guid correlationId,
        int page,
        int pageSize,
        Expression<Func<TEntity, TResult>> selector,
        CancellationToken cancellationToken,
        string? sortBy = null!,
        bool sortDescending = false,
        string searchQuery = null!,
        string searchValue = null!,
        string searchLanguage = "english",
        Expression<Func<TEntity, bool>> predicate = null!,
        bool? newContext = null
    ) where TEntity : DomainEntity => await HandleBaseQueryAsync<TEntity, (IEnumerable<TResult> Items, int TotalRecords)>(async dbEntitySet =>
    {
        var totalRecords = _dbContextFactory
            .CreateDbContext()
            .Set<TEntity>()
            .CountAsync(cancellationToken);

        var query = dbEntitySet.AsQueryable();

        if (predicate != null)
            query = query.Where(predicate);

        if (!string.IsNullOrWhiteSpace(sortBy))
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        else
            query = query.OrderBy(e => e.CreatedAt);

        if (!string.IsNullOrWhiteSpace(searchQuery) && !string.IsNullOrWhiteSpace(searchValue))
            query = query.Where(e => EF.Functions.ToTsVector(searchLanguage, EF.Property<string>(e, searchQuery)).Matches(searchValue));

        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(selector)
            .ToListAsync(cancellationToken);

        await Task.WhenAll(totalRecords, items);

        return (await items, await totalRecords);
    }, correlationId, newContext);
}
