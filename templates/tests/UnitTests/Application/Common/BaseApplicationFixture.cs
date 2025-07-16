using Application.Common.Repositories;
using CommonTests.Fixtures;
using Domain.Common;
using FluentValidation;
using FluentValidation.Results;
using Serilog;

namespace UnitTests.Application.Common;

public class BaseApplicationFixture<TEntity, TRequest> : BaseFixture where TEntity : DomainEntity where TRequest : class
{
    public Mock<IServiceProvider> mockServiceProvider = new();
    public Mock<ILogger> mockLogger = new();
    public Mock<IBaseRepository<TEntity>> mockRepository = new();
    public Mock<IValidator<TRequest>> mockValidator = new();

    public void MockServiceProviderServices()
    {
        mockServiceProvider
            .Setup(r => r.GetService(typeof(ILogger)))
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

    public void MockRepository<TResult>(TResult result) => mockRepository
        .Setup(d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()));

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

    public void VerifyLoggerError<TLoggerObjectType>(int times)
    {
        mockLogger.Verify(
            l => l.Error(It.IsAny<string>(), It.IsAny<TLoggerObjectType>()),
            Times.Exactly(times)
        );
    }

    public void VerifyLoggerInformation(int times)
    {
        mockLogger.Verify(
            l => l.Information(It.IsAny<string>()),
            Times.Exactly(times)
        );
    }

    public void VerifyRepository(int times)
    {
        mockRepository.Verify(
            d => d.AddAsync(It.IsAny<TEntity>(), It.IsAny<CancellationToken>()),
            Times.Exactly(times)
        );
    }
}
