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
    public TUseCase? useCase;

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
        var validationResult = new ValidationResult()
        {
            Errors = [
                new ValidationFailure("Description", "Description is required")
            ]
        };
        mockValidator
            .Setup(v => v.ValidateAsync(request, cancellationToken))
            .ReturnsAsync(validationResult);
    }

    public void VerifyStartUseCaseLog(string className, Guid correlationId, int times = 1) => mockLogger.Verify(
        l => l.LogInformation(
            "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Start to execute use case",
            className, "Handle", correlationId
        ),
        Times.Exactly(times)
    );

    public void VerifyFinishUseCaseLog(string className, Guid correlationId, int times = 1) => mockLogger.Verify(
        l => l.LogInformation(
            "[{ClassName}] | [{MethodName}] | [{CorrelationId}] | Finished executing use case with success",
            className, "HandleInternalAsync", correlationId
        ),
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
