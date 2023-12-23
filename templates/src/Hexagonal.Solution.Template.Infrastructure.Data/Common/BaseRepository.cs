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

    public async Task<TEntity> Add(TEntity entity)
    {
        await dbEntitySet.AddAsync(entity);
        return entity;
    }

    public async Task AddRange(TEntity[] entities) =>
        await dbEntitySet.AddRangeAsync(entities);

    public void BeginTransaction()
    {
        if (dbContext.Database.CurrentTransaction == null)
            dbContext.Database.BeginTransaction();
    }

    public void CommitTransaction()
    {
        if (dbContext.Database.CurrentTransaction != null)
        {
            dbContext.SaveChanges();
            dbContext.Database.CurrentTransaction.Commit();
        }
    }

    public void RollbackTransaction()
    {
        if (dbContext.Database.CurrentTransaction != null)
            dbContext.Database.CurrentTransaction.Rollback();
    }

    public async Task<bool> CheckExistsByWhere(Expression<Func<TEntity, bool>> predicate)
        => await dbEntitySet.AnyAsync(predicate);

    public async Task<bool> CheckExistsByWhereAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        => await dbEntitySet.AsNoTracking().AnyAsync(predicate);

    public async Task<TEntity?> GetById(object id) => await dbEntitySet.FindAsync(id);

    public async Task<IList<TEntity>> GetByWhere(Expression<Func<TEntity, bool>> predicate)
        => await dbEntitySet.Where(predicate).ToListAsync();

    public async Task<IList<TEntity>> GetByWhereAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        => await dbEntitySet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<TEntity?> FirstOrDefaultAsNoTracking(Expression<Func<TEntity, bool>> predicate)
        => await dbEntitySet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public void Update(TEntity entity) => dbEntitySet.Update(entity);

    public void Remove(TEntity entity) => dbEntitySet.Remove(entity);

    public void RemoveRange(TEntity[] entities) => dbEntitySet.RemoveRange(entities);

    public async Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate)
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity);
        else
            dbEntitySet.Update(entity);

        return entity;
    }
}