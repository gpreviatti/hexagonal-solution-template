using Application.Common.Messages;
using Application.Common.Repositories;
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
    public Mock<IServiceProvider> MockServiceProvider { get; } = new();
    public Mock<ILogger> MockLogger { get; } = new();
    public Mock<ILoggerFactory> MockLoggerFactory { get; } = new();
    public Mock<IProduceService> MockProduceService { get; } = new();
    public Mock<IBaseRepository> MockRepository { get; } = new();
    public Mock<IValidator<TRequest>> MockValidator { get; } = new();
    public Mock<IHybridCacheService> MockCache { get; } = new();
    public TUseCase UseCase { get; set; } = default!;

    public BaseApplicationFixture() => MockServiceProviderServices();

    private void MockServiceProviderServices()
    {
        MockServiceProvider
            .Setup(r => r.GetService(typeof(ILoggerFactory)))
            .Returns(MockLoggerFactory.Object);

        MockLogger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        MockLoggerFactory
            .Setup(l => l.CreateLogger(It.IsAny<string>()))
            .Returns(MockLogger.Object);

        MockServiceProvider
            .Setup(r => r.GetService(typeof(IValidator<TRequest>)))
            .Returns(MockValidator.Object);

        MockServiceProvider
            .Setup(r => r.GetService(typeof(IHybridCacheService)))
            .Returns(MockCache.Object);

        MockServiceProvider
            .Setup(r => r.GetService(typeof(IProduceService)))
            .Returns(MockProduceService.Object);

        MockServiceProvider
            .Setup(r => r.GetService(typeof(IBaseRepository)))
            .Returns(MockRepository.Object);
    }

    public void ClearInvocations()
    {
        MockLogger.Invocations.Clear();
        MockValidator.Reset();
        MockCache.Reset();
        MockProduceService.Reset();
        MockRepository.Reset();
    }

    public BasePaginatedRequest SetValidBasePaginatedRequest() => new(Guid.NewGuid(), 1, 10);

    public void SetSuccessfulValidator(TRequest request)
    {
        var validationResult = new ValidationResult();
        MockValidator
            .Setup(v => v.ValidateAsync(request, CancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetFailedValidator(TRequest request)
    {
        ValidationResult validationResult = new()
        {
            Errors = [new("Description", "Description is required")]
        };
        MockValidator
            .Setup(v => v.ValidateAsync(request, CancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void SetValidGetOrCreateAsync<TResult>(TResult result) => MockCache
        .Setup(c => c.GetOrCreateAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
            It.IsAny<CancellationToken>()
    )).ReturnsAsync(result);

    public void SetInvalidGetOrCreateAsync<TResult>() => MockCache.Setup(c => c.GetOrCreateAsync(
        It.IsAny<Guid>(),
        It.IsAny<string>(),
        It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
        It.IsAny<CancellationToken>()
    ));

    public void VerifyCache<TResult>(int times) => MockCache.Verify(
        c => c.GetOrCreateAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Func<CancellationToken, ValueTask<TResult>>>(),
            It.IsAny<CancellationToken>()
        ),
        Times.Exactly(times)
    );

    public void VerifyProduce<TMessage>(int times = 1) where TMessage : BaseMessage => MockProduceService.Verify(
        p => p.HandleAsync(
            It.IsAny<TMessage>(),
            It.IsAny<CancellationToken>(),
            It.IsAny<string>(),
            It.IsAny<string>()
        ),
        Times.Exactly(times)
    );
}
