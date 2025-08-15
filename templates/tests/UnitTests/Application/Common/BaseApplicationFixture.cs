using Application.Common.Repositories;
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
    }

    public void ClearInvocations()
    {
        mockLogger.Invocations.Clear();
        mockRepository.Invocations.Clear();
        mockValidator.Invocations.Clear();
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

    public void VerifyStartUseCaseLog(int times = 1) => mockLogger.VerifyLog(
        l => l.LogInformation("*Start to execute use case*"),
        Times.Exactly(times)
    );

    public void VerifyFinishUseCaseLog(int times = 1) => mockLogger.VerifyLog(
        l => l.LogInformation("*Finished executing use case with success*"),
        Times.Exactly(times)
    );

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
}
