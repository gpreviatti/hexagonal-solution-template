using Application.Common.Constants;
using Application.Common.Messages;
using Domain.Notifications;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Messaging.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.WebApp.Messaging.Notifications;

public class CreateNotificationTestFixture : BaseMessagingFixture
{
    public new void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        SetServices(scope);
    }

    public CreateNotificationMessage SetValidMessage() => autoFixture.Build<CreateNotificationMessage>().Create();
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateNotificationTest : IClassFixture<CreateNotificationTestFixture>
{
    private readonly CreateNotificationTestFixture _fixture;

    public CreateNotificationTest(CustomWebApplicationFactory<Program> factory, CreateNotificationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetServices(factory);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Message_Then_Pass))]
    public async Task Given_A_Valid_Message_Then_Pass()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);

        var notification = await _fixture.repository.FirstOrDefaultAsNoTrackingAsync<Notification>(
            Guid.NewGuid(),
            n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(message.NotificationType, notification.NotificationType);
    }

    [Fact(DisplayName = nameof(Given_A_Duplicate_Message_Then_Should_Not_Create_Duplicated_Message))]
    public async Task Given_A_Duplicate_Message_Then_Should_Not_Create_Duplicated_Message()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);

        var notifications = await _fixture.repository.GetByWhereAsNoTrackingAsync<Notification>(
            Guid.NewGuid(),
            n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus,
            cancellationToken: _fixture.cancellationToken
        );

        // Assert
        Assert.Single(notifications);
    }
}
