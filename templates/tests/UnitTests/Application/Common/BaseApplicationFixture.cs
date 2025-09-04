using System.Linq.Expressions;
using Application.Common.Repositories;
using Application.Common.Services;
using CommonTests.Fixtures;
using Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.Common;

public class BaseApplicationFixture<TEntity, TRequest, TUseCase> : BaseFixture
    where TEntity : DomainEntity
    where TRequest : class
    where TUseCase : class
{
    public Mock<IServiceProvider> mockServiceProvider = new();
    public Mock<ILogger<TUseCase>> mockLogger = new();
    public Mock<IBaseRepository<TEntity>> mockRepository = new();
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
            .Setup(r => r.GetService(typeof(IBaseRepository<TEntity>)))
            .Returns(mockRepository.Object);

        mockServiceProvider
            .Setup(r => r.GetService(typeof(IHybridCacheService)))
            .Returns(mockCache.Object);
    }

    public void ClearInvocations()
    {
        mockLogger.Invocations.Clear();
        mockRepository.Invocations.Clear();
        mockValidator.Invocations.Clear();
        mockCache.Invocations.Clear();
    }

    public void SetSuccessfulAddAsync() => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(1);

    public void SetFailedAddAsync() => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()))
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

    public void SetInvalidGetOrCreateAsync<TResult>() => mockCache.Setup(c => c.GetOrCreateAsync(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
        It.IsAny<CancellationToken>()
    ));

    public void SetValidGetAllPaginatedAsync(IEnumerable<TEntity> orders, int totalRecords) => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string>(),
        It.IsAny<bool>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Expression<Func<TEntity, object>>>()
    )).ReturnsAsync((orders, totalRecords));

    public void SetInvalidGetAllPaginatedAsync<T>() where T : class => mockRepository.Setup(r => r.GetAllPaginatedAsync(
        It.IsAny<int>(),
        It.IsAny<int>(),
        It.IsAny<CancellationToken>(),
        It.IsAny<string>(),
        It.IsAny<bool>(),
        It.IsAny<string>(),
        It.IsAny<string>(),
        It.IsAny<Expression<Func<TEntity, object>>>()
    )).ReturnsAsync((null, 0));

    public void VerifyStartUseCaseLog(int times = 1) => VerifyLogInformation("Start to execute use case", times);
    public void VerifyFinishUseCaseLog(int times = 1) => VerifyLogInformation("Finished executing use case with success", times);

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

    public void VerifyRepository(int times)
    {
        mockRepository.Verify(
            d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(times)
        );
    }
    
    public void VerifyCache<TResult>(int times)
    {
        mockCache.Verify(
            c => c.GetOrCreateAsync(
                It.IsAny<string>(),
                It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
                It.IsAny<CancellationToken>()
            ),
            Times.Exactly(times)
        );
    }
}
