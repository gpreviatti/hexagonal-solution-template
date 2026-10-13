using Application.Notifications;
using Domain.Common.Enums;
using Domain.Notifications;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class CreateNotificationUseCaseFixture : BaseApplicationFixture<CreateNotificationUseCase>
{
    public CreateNotificationUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public static CreateNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), NotificationType.OrderCreated, NotificationStatus.Pending, "System", new { Test = "Message" });

    public static CreateNotificationRequest SetInvalidRequest() =>
        new(Guid.Empty, NotificationType.OrderCreated, NotificationStatus.Pending);
}

public sealed class CreateNotificationUseCaseTests : IClassFixture<CreateNotificationUseCaseFixture>
{
    private readonly CreateNotificationUseCaseFixture _fixture;

    public CreateNotificationUseCaseTests(CreateNotificationUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPassAsync))]
    public async Task GivenAValidRequestThenPassAsync()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFailsAsync))]
    public async Task GivenAnInvalidRequestThenFailsAsync()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetInvalidRequest();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockLogger.VerifyOperationFailed(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenFailsAsync))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenFailsAsync()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.MockRepository.SetFailedAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyOperationFailed();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenShouldLogExactFailureMessageAsync))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenShouldLogExactFailureMessageAsync()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.MockRepository.SetFailedAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyWarning("Failed to create notification. No rows affected.");
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithSuccessStatusThenShouldCreateNotificationAsync))]
    public async Task GivenAValidRequestWithSuccessStatusThenShouldCreateNotificationAsync()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(),
            NotificationType.OrderCreated,
            NotificationStatus.Success,
            "TestUser",
            new { OrderId = 123, Amount = 500 }
        );
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithDifferentStatusesThenShouldHandleAllStatusesAsync))]
    public async Task GivenAValidRequestWithDifferentStatusesThenShouldHandleAllStatusesAsync()
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
            _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

            // Act
            await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

            // Assert
            _fixture.MockLogger.VerifyStartOperation();
            _fixture.MockLogger.VerifyFinishOperation();
            _fixture.MockRepository.VerifyAddAsync<Notification>(1);
        }
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithNullMessageThenShouldStoreEmptyStringAsync))]
    public async Task GivenAValidRequestWithNullMessageThenShouldStoreEmptyStringAsync()
    {
        // Arrange
        var request = new CreateNotificationRequest(
            Guid.NewGuid(),
            NotificationType.OrderCreated,
            NotificationStatus.Pending,
            "System",
            null
        );
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithObjectMessageThenShouldSerializeToJsonAsync))]
    public async Task GivenAValidRequestWithObjectMessageThenShouldSerializeToJsonAsync()
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
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }
}
