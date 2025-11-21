using System.Linq.Expressions;
using Application.Common.Messages;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.Services;
using CommonTests.Fixtures;
using Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.Common;

public class BaseApplicationFixture<TRequest, TUseCase> : BaseFixture
    where TRequest : class
    where TUseCase : class
{
    public Mock<IServiceProvider> mockServiceProvider = new();
    public Mock<ILogger<TUseCase>> mockLogger = new();
    public Mock<IBaseRepository> mockRepository = new();
    public Mock<IProduceService> mockProduceService = new();
    public Mock<IValidator<TRequest>> mockValidator = new();
    public Mock<IHybridCacheService> mockCache = new();
    public TUseCase useCase = default!;

    public void MockServiceProviderServices()
    {
        mockServiceProvider
            .Setup(r => r.GetService(typeof(ILogger<TUseCase>)))
            .Returns(mockLogger.Object);

        mockServiceProvider
        .Setup(r => r.GetService(typeof(IValidator<TRequest>)))
        .Returns(mockValidator.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(IBaseRepository)))
            .Returns(mockRepository.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(IHybridCacheService)))
            .Returns(mockCache.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(IProduceService)))
            .Returns(mockProduceService.Object);
    }

    public void ClearInvocations()
    {
        mockLogger.Reset();
        mockRepository.Reset();
        mockValidator.Reset();
        mockCache.Reset();
        mockProduceService.Reset();
    }

    public BasePaginatedRequest SetValidBasePaginatedRequest() => new(Guid.NewGuid(), 1, 10);

    public void SetSuccessfulAddAsync<TEntity>() where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public void SetFailedAddAsync<TEntity>() where TEntity : DomainEntity => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public void SetSuccessfulValidator(TRequest request)
    {
        var validationResult = new ValidationResult();
        mockValidator
            .Setup(v => v.ValidateAsync(request, cancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetFailedValidator(TRequest request)
    {
        ValidationResult validationResult = new()
        {
            Errors = [new("Description", "Description is required")]
        };
        mockValidator
            .Setup(v => v.ValidateAsync(request, cancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetValidGetOrCreateAsync<TResult>(TResult result) => mockCache.Setup(c => c.GetOrCreateAsync(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
        It.IsAny<CancellationToken>()
    )).ReturnsAsync(result);

    public void SetupGetByIdAsNoTrackingAsync<TEntity>(TEntity entity) where TEntity : DomainEntity => mockRepository.Setup(r => r.GetByIdAsNoTrackingAsync(
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync(entity);

    public void SetupGetByIdAsNoTrackingAsyncNotFound<TEntity>() where TEntity : DomainEntity => mockRepository.Setup(r => r.GetByIdAsNoTrackingAsync(
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync((TEntity)null!);

    public void SetSuccessfulUpdate<TEntity>() where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public void SetFailedUpdate<TEntity>() where TEntity : DomainEntity => mockRepository
        .Setup(d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(0);

    public void SetInvalidGetOrCreateAsync<TResult>() => mockCache.Setup(c => c.GetOrCreateAsync(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
        It.IsAny<CancellationToken>()
    ));

    public void SetValidGetAllPaginatedAsyncNoIncludes<TEntity>(IEnumerable<TEntity> entities, int totalRecords) where TEntity : DomainEntity => mockRepository.Setup(r => r.GetAllPaginatedAsync<TEntity>(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>>()
    )).ReturnsAsync((entities, totalRecords));

    public void SetInvalidGetAllPaginatedAsync<TEntity>() where TEntity : DomainEntity => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string?>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>?>(),
        It.IsAny<Expression<Func<TEntity, object>>[]>()
    )).ReturnsAsync(([], 0));

    public void VerifyStartUseCaseLog(int times = 1) => VerifyLogInformation("Start to execute use case", times);
    public void VerifyFinishUseCaseLog(int times = 1) => VerifyLogInformation("Finished executing use case", times);
    public void VerifyFinishUseCaseWithCacheLog(int times = 1) => VerifyLogInformation("Finished executing use case with cache key", times);

    public void VerifyLogInformation(string message, int times = 1) => mockLogger.VerifyLog(
        l => l.LogInformation($"*{message}*"),
        Times.Exactly(times)
    );

    public void VerifyLogWarning(string message, int times = 1) => mockLogger.VerifyLog(
        l => l.LogWarning($"*{message}*"),
        Times.Exactly(times)
    );

    public void VerifyLogError(string message, int times = 1) => mockLogger.VerifyLog(
        l => l.LogError($"*{message}*"),
        Times.Exactly(times)
    );

    public void VerifyAddAsync<TEntity>(int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );

    public void VerifyUpdate<TEntity>(int times) where TEntity : DomainEntity => mockRepository.Verify(
        d => d.UpdateAsync(It.IsAny<TEntity>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
        Times.Exactly(times)
    );

    public void VerifyGetAllPaginatedNoIncludes<TEntity>(int times) where TEntity : DomainEntity => mockRepository.Verify(r => r.GetAllPaginatedAsync<TEntity>(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string>(),
        It.IsAny<bool>(),
        It.IsAny<Dictionary<string, string>>()
    ), Times.Exactly(times));

    public void VerifyGetByIdAsync<TEntity>(int times) where TEntity : DomainEntity => mockRepository.Verify(r => r.GetByIdAsNoTrackingAsync(
        It.IsAny<int>(),
        It.IsAny<Guid>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<Expression<Func<TEntity, object>>[]>()
    ), Times.Exactly(times));

    public void VerifyCache<TResult>(int times) => mockCache.Verify(
        c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
            It.IsAny<CancellationToken>()
        ),
        Times.Exactly(times)
    );

    public void VerifyProduce<TMessage>(int times = 1) where TMessage : BaseMessage => mockProduceService.Verify(
        p => p.HandleAsync(
            It.IsAny<TMessage>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ),
        Times.Exactly(times)
    );
}
