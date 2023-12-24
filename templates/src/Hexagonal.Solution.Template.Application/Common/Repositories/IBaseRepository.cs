using Hexagonal.Solution.Template.Domain.Common;
using System.Linq.Expressions;

namespace Hexagonal.Solution.Template.Application.Common.Repositories;
public interface IBaseRepository<TEntity> where TEntity : DomainEntity
{
    Task<TEntity> AddAsync(TEntity entity, CancellationToken cancellationToken);
    Task AddRangeAsync(TEntity[] entities, CancellationToken cancellationToken);
    Task<TEntity> AddOrUpdateIfNotExistsAsync(TEntity entity, Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    
    void Update(TEntity entity);

    void RemoveAsync(TEntity entity);
    void RemoveRangeAsync(TEntity[] entities);

    Task<bool> CheckExistsByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<bool> CheckExistsByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity?> FirstOrDefaultAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<TEntity?> GetByIdAsNoTrackingAsync(object id, CancellationToken cancellationToken);
    Task<IList<TEntity>> GetByWhereAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);
    Task<IList<TEntity>> GetByWhereAsNoTrackingAsync(Expression<Func<TEntity, bool>> predicate, CancellationToken cancellationToken);

    Task BeginTransactionAsync(CancellationToken cancellationToken);
    Task CommitTransactionAsync(CancellationToken cancellationToken);
    Task RollbackTransactionAsync(CancellationToken cancellationToken);
}