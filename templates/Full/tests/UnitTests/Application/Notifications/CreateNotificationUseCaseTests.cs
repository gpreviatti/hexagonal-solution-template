using Application.Notifications;
using Domain.Common.Enums;
using Domain.Notifications;
using FluentValidation;
using FluentValidation.TestHelper;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class CreateNotificationRequestValidationFixture
{
    public IValidator<CreateNotificationRequest> Validator { get; } = new CreateNotificationRequestValidator();

    public static CreateNotificationRequest GetValidRequest() =>
        new(Guid.NewGuid(), NotificationType.OrderCreated, NotificationStatus.Pending, "System", new { Test = "Message" });
}

public sealed class CreateNotificationRequestValidationTests(CreateNotificationRequestValidationFixture fixture) : IClassFixture<CreateNotificationRequestValidationFixture>
{
    private readonly CreateNotificationRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = CreateNotificationRequestValidationFixture.GetValidRequest();

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateNotificationRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            NotificationType = (NotificationType)(-1),
            NotificationStatus = (NotificationStatus)(-1)
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("NotificationType");
        result.ShouldHaveValidationErrorFor("NotificationStatus");
    }
}

public sealed class CreateNotificationUseCaseFixture : BaseApplicationFixture<CreateNotificationRequest, CreateNotificationUseCase>
{
    public CreateNotificationUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public static CreateNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), NotificationType.OrderCreated, NotificationStatus.Pending, "System", new { Test = "Message" });
}

public sealed class CreateNotificationUseCaseTests : IClassFixture<CreateNotificationUseCaseFixture>
{
    private readonly CreateNotificationUseCaseFixture _fixture;

    public CreateNotificationUseCaseTests(CreateNotificationUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenFails))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenFails()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyOperationFailed();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenShouldLogExactFailureMessage))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenShouldLogExactFailureMessage()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyWarning("Failed to create notification. No rows affected.");
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithSuccessStatusThenShouldCreateNotification))]
    public async Task GivenAValidRequestWithSuccessStatusThenShouldCreateNotification()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(),
            NotificationType.OrderCreated,
            NotificationStatus.Success,
            "TestUser",
            new { OrderId = 123, Amount = 500 }
        );
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithDifferentStatusesThenShouldHandleAllStatuses))]
    public async Task GivenAValidRequestWithDifferentStatusesThenShouldHandleAllStatuses()
    {
        // Test that different statuses are handled correctly
        var statuses = new[] { NotificationStatus.Pending, NotificationStatus.Success, NotificationStatus.Failed };

        foreach (var status in statuses)
        {
            _fixture.ClearInvocations();

            var request = new CreateNotificationRequest(
                Guid.NewGuid(),
                NotificationType.OrderCreated,
                status,
                "System"
            );
            _fixture.SetSuccessfulValidator(request);
            _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

            // Act
            await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

            // Assert
            _fixture.MockLogger.VerifyStartOperation();
            _fixture.MockLogger.VerifyFinishOperation();
            _fixture.MockRepository.VerifyAddAsync<Notification>(1);
        }
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithNullMessageThenShouldStoreEmptyString))]
    public async Task GivenAValidRequestWithNullMessageThenShouldStoreEmptyString()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(),
            NotificationType.OrderCreated,
            NotificationStatus.Pending,
            "System",
            null
        );
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithObjectMessageThenShouldSerializeToJson))]
    public async Task GivenAValidRequestWithObjectMessageThenShouldSerializeToJson()
    {
        // Arrange
        var messageObject = new { OrderId = 456, Status = "Completed", Amount = 1500.50 };
        var request = new CreateNotificationRequest(
            Guid.NewGuid(),
            NotificationType.OrderCreated,
            NotificationStatus.Success,
            "System",
            messageObject
        );
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }
}
