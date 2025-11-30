using System.Linq.Expressions;
using Application.Common.Repositories;
using Domain.Common;

namespace UnitTests.Application.Common;

public static class RepositoryMockExtensions
{
    public static void SetSuccessfulAddAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public static void SetFailedAddAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public static void SetupGetByIdAsNoTrackingAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, TEntity entity) where TEntity : DomainEntity => mockRepository
        .Setup(r => r.GetByIdAsNoTrackingAsync(
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync(entity);

    public static void SetupGetByIdAsNoTrackingAsync<TEntity, TResult>(this Mock<IBaseRepository<TEntity>> mockRepository, TResult result) where TEntity : DomainEntity => mockRepository
        .Setup(r => r.GetByIdAsNoTrackingAsync(
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<Expression<Func<TEntity, TResult>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync(result);

    public static void SetupGetByIdAsNoTrackingAsyncNotFound<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(r => r.GetByIdAsNoTrackingAsync(
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync((TEntity) null!);

    public static void SetupGetByIdAsNoTrackingAsyncNotFound<TEntity, TResult>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(r => r.GetByIdAsNoTrackingAsync(
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<Expression<Func<TEntity, TResult>>>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<TEntity, object>>[]>()
    ));
    

    public static void VerifyAddAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );

    public static void VerifyUpdate<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );
    
    public static void SetSuccessfulUpdate<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public static void SetFailedUpdate<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public static void SetValidGetAllPaginatedAsyncNoIncludes<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, IEnumerable<TEntity> entities, int totalRecords) where TEntity : DomainEntity => mockRepository.Setup(r => r.GetAllPaginatedAsync<TEntity>(
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>>()
    )).ReturnsAsync((entities, totalRecords));

    public static void SetInvalidGetAllPaginatedAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository) where TEntity : DomainEntity => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<Guid>(),
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string?>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>?>(),
        It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync(([], 0));


    public static void VerifyGetAllPaginatedNoIncludes<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, int times) where TEntity : DomainEntity => mockRepository
        .Verify(r => r.GetAllPaginatedAsync<TEntity>(
            It.IsAny<Guid>(),
            It.IsAny<int>(),
            It.IsAny<int>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string>(),
            It.IsAny<bool>(),
            It.IsAny<Dictionary<string, string>?>()
    ), Times.Exactly(times));

    public static void VerifyGetByIdAsync<TEntity>(this Mock<IBaseRepository<TEntity>> mockRepository, int times) where TEntity : DomainEntity => mockRepository
        .Verify(r => r.GetByIdAsNoTrackingAsync(
            It.IsAny<int>(),
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<Expression<Func<TEntity, object>>[]>()
    ), Times.Exactly(times));
}