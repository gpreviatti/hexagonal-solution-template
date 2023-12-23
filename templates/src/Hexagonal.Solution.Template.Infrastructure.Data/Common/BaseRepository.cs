using Hexagonal.Solution.Template.Application.Common.Repositories;
using Hexagonal.Solution.Template.Domain.Common;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Common;
public abstract class BaseRepository<TEntity> : IBaseRepository<TEntity> where TEntity : DomainEntity
{
    protected readonly DbContext dbContext;
    protected readonly DbSet<TEntity> dbEntitySet;

    public BaseRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
        dbEntitySet = this.dbContext.Set<TEntity>();
    }

    public async Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken)
    {
        await dbEntitySet.AddAsync(entity, cancellationToken);
        return entity;
    }

    public async Task AddRangeAsync(TEntity[] entities, CancellationToken cancellationToken) =>
        await dbEntitySet.AddRangeAsync(entities, cancellationToken);

    public async Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity, cancellationToken);
        else
            dbEntitySet.Update(entity);

        return entity;
    }


    public void Update(TEntity entity) => dbEntitySet.Update(entity);


    public void RemoveAsync(TEntity entity) => dbEntitySet.Remove(entity);
    public void RemoveRangeAsync(TEntity[] entities) => dbEntitySet.RemoveRange(entities);


    public async Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AnyAsync(predicate, cancellationToken);

    public async Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().AnyAsync(predicate, cancellationToken);

    public async Task<TEntity?> GetByIdAsync(object id) => await dbEntitySet.FindAsync(id);

    public async Task<IList<TEntity>> GetByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.Where(predicate).ToListAsync(cancellationToken);

    public async Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().Where(predicate).ToListAsync(cancellationToken);

    public async Task<TEntity?> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken)
        => await dbEntitySet.AsNoTracking().FirstOrDefaultAsync(predicate, cancellationToken);


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