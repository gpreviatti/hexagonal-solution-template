using Application.Common.Messages;
using Application.Common.Requests;
using Application.Common.Services;
using CommonTests.Fixtures;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace UnitTests.Application.Common;

public class BaseApplicationFixture<TRequest, TUseCase> : BaseFixture
    where TRequest : class
    where TUseCase : class
{
    public Mock<IServiceProvider> mockServiceProvider = new();
    public Mock<ILogger> mockLogger = new();
    public Mock<ILoggerFactory> mockLoggerFactory = new();
    public Mock<IProduceService> mockProduceService = new();
    public Mock<IValidator<TRequest>> mockValidator = new();
    public Mock<IHybridCacheService> mockCache = new();
    public TUseCase useCase = default!;

    public void MockServiceProviderServices()
    {
        mockServiceProvider
            .Setup(r => r.GetService(typeof(ILoggerFactory)))
            .Returns(mockLoggerFactory.Object);

        mockLoggerFactory
            .Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns(mockLogger.Object);

        mockServiceProvider
        .Setup(r => r.GetService(typeof(IValidator<TRequest>)))
        .Returns(mockValidator.Object);

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
        mockValidator.Reset();
        mockCache.Reset();
        mockProduceService.Reset();
    }

    public BasePaginatedRequest SetValidBasePaginatedRequest() => new(Guid.NewGuid(), 1, 10);

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

    public void SetValidGetOrCreateAsync<TResult>(TResult result) => mockCache
        .Setup(c => c.GetOrCreateAsync(
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
            It.IsAny<CancellationToken>()
    )).ReturnsAsync(result);

    public void SetInvalidGetOrCreateAsync<TResult>() => mockCache.Setup(c => c.GetOrCreateAsync(
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
        It.IsAny<CancellationToken>()
    ));

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
