using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Hexagonal.Solution.Template.Infrastructure.Data.Common;
public abstract class BaseRepository<T> : IBaseRepository<T> where T : class, new()
{
    protected readonly DbContext dbContext;
    protected readonly DbSet<T> dbEntitySet;

    public BaseRepository(DbContext dbContext)
    {
        this.dbContext = dbContext;
        dbEntitySet = this.dbContext.Set<T>();
    }

    public async Task<T> Add(T entity)
    {
        await dbEntitySet.AddAsync(entity);
        return entity;
    }

    public async Task AddRange(T[] entities) =>
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

    public async Task<bool> CheckExistsByWhere(Expression<Func<T, bool>> predicate)
        => await dbEntitySet.AnyAsync(predicate);

    public async Task<bool> CheckExistsByWhereAsNoTracking(Expression<Func<T, bool>> predicate)
        => await dbEntitySet.AsNoTracking().AnyAsync(predicate);

    public async Task<T?> GetById(object id) => await dbEntitySet.FindAsync(id);

    public async Task<IList<T>> GetByWhere(Expression<Func<T, bool>> predicate)
        => await dbEntitySet.Where(predicate).ToListAsync();

    public async Task<IList<T>> GetByWhereAsNoTracking(Expression<Func<T, bool>> predicate)
        => await dbEntitySet.AsNoTracking().Where(predicate).ToListAsync();

    public async Task<T?> FirstOrDefaultAsNoTracking(Expression<Func<T, bool>> predicate)
        => await dbEntitySet.AsNoTracking().FirstOrDefaultAsync(predicate);

    public void Update(T entity) => dbEntitySet.Update(entity);

    public void Remove(T entity) => dbEntitySet.Remove(entity);

    public void RemoveRange(T[] entities) => dbEntitySet.RemoveRange(entities);

    public async Task<T> AddOrUpdateIfNotExistsAsync(T entity, Expression<Func<T, bool>> predicate)
    {
        var exists = dbEntitySet.AsNoTracking().Any(predicate);

        if (!exists)
            await dbEntitySet.AddAsync(entity);
        else
            dbEntitySet.Update(entity);

        return entity;
    }
}