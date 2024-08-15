using System.Linq.Expressions;
using Application.Common.Repositories;
using Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data.Common;
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : DomainEntity
{
    protected readonly DbContext dbContext;
    protected readonly DbSet<TEntity> dbEntitySet;

    public BaseRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
        dbEntitySet = this.dbContext.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await dbEntitySet.AddAsync(entity, cancellationToken);

        await dbContext.SaveChangesAsync();
    }

    public async Task AddRangeAsync(TEntity[] entities, CancellationToken cancellationToken)
    {
        await dbEntitySet.AddRangeAsync(entities, cancellationToken);

        await dbContext.SaveChangesAsync();
    }

    public async Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity, cancellationToken);
        else
            dbEntitySet.Update(entity);

        await dbContext.SaveChangesAsync();

        return entity;
    }


    public void Update(TEntity entity)
    {
        dbEntitySet.Update(entity);

        dbContext.SaveChanges();
    }

    public void RemoveAsync(TEntity entity)
    {
        dbEntitySet.Remove(entity);

        dbContext.SaveChanges();
    }

    public void RemoveRangeAsync(TEntity[] entities)
    {
        dbEntitySet.RemoveRange(entities);

        dbContext.SaveChanges();
    }

    public async Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AnyAsync(predicate, cancellationToken);

    public async Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<TEntity> GetByIdAsNoTrackingAsync(object id, CancellationToken cancellationToken) => await dbEntitySet
        .AsNoTracking()
        .FirstAsync(e => e.Id.Equals(id), cancellationToken);

    public async Task<IList<TEntity>> GetByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken) ?? default!;

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
}
