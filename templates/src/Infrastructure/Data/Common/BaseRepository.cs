using System.Linq.Expressions;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Common;

public class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : DomainEntity
{
    protected readonly MyDbContext dbContext;
    protected readonly DbSet<TEntity> dbEntitySet;

    public BaseRepository(MyDbContext dbContext)
    {
        this.dbContext = dbContext;
        dbEntitySet = this.dbContext.Set<TEntity>();
    }

    public async Task<int> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await dbEntitySet.AddAsync(entity, cancellationToken);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> AddRangeAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        await dbEntitySet.AddRangeAsync(entities, cancellationToken);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity, cancellationToken);
        else
            dbEntitySet.Update(entity);

        await dbContext.SaveChangesAsync(cancellationToken);

        return entity;
    }

    public int Update(TEntity entity)
    {
        dbEntitySet.Update(entity);

        return dbContext.SaveChanges();
    }

    public async Task<int> RemoveAsync(TEntity entity, CancellationToken cancellationToken)
    {
        dbEntitySet.Remove(entity);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<int> RemoveRangeAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        dbEntitySet.RemoveRange(entities);

        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AnyAsync(predicate, cancellationToken);

    public async Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<TEntity> GetByIdAsNoTrackingAsync(
        int id,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(o => o.Id == id, cancellationToken) ?? default!;
    }
    public async Task<IList<TEntity>> GetByWhereAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.Where(predicate).ToListAsync(cancellationToken);
    }

    public async Task<TEntity> FirstOrDefaultAsNoTrackingAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        query = SetIncludes(includes, query);

        return await query.FirstOrDefaultAsync(predicate, cancellationToken) ?? default!;
    }

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

    public async Task<(IEnumerable<TEntity> Items, int TotalRecords)> GetAllPaginatedAsync(
        int page,
        int pageSize,
        CancellationToken cancellationToken,
        string sortBy = null!,
        bool sortDescending = false,
        string searchPropertyName = null!,
        string searchValue = null!,
        params Expression<Func<TEntity, object>>[] includes
    )
    {
        IQueryable<TEntity> query = dbEntitySet.AsNoTracking();

        if (includes != null && includes.Length > 0)
        {
            foreach (var include in includes)
                query = query.Include(include);
        }

        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            query = sortDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, sortBy))
                : query.OrderBy(e => EF.Property<object>(e, sortBy));
        }

        var totalRecords = await query.CountAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(searchPropertyName) && !string.IsNullOrWhiteSpace(searchValue))
        {
            query = query.Where(e => EF.Property<string>(e, searchPropertyName)
                .Contains(searchValue, StringComparison.OrdinalIgnoreCase));
        }
        
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalRecords);
    }

    private static IQueryable<TEntity> SetIncludes(
        Expression<Func<TEntity, object>>[] includes,
        IQueryable<TEntity> query
    )
    {
        if (includes != null && includes.Length > 0)
            foreach (var include in includes)
                query = query.Include(include);

        return query;
    }
}
