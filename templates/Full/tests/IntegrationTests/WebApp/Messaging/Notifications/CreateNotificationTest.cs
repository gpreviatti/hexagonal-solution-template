using Application.Common.Constants;
using Application.Common.Messages;
using Domain.Notifications;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Messaging.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.WebApp.Messaging.Notifications;

public class CreateNotificationTestFixture : BaseMessagingFixture
{
    public new void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        SetServices(scope);
    }

    public CreateNotificationMessage SetValidMessage() => AutoFixture.Build<CreateNotificationMessage>().Create();
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

    [Fact(DisplayName = nameof(GivenAValidMessageThenPass))]
    public async Task GivenAValidMessageThenPass()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);

        var notification = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus)
            .FirstOrDefaultAsync(_fixture.CancellationToken);

        // Assert
        Assert.NotNull(notification);
        Assert.Equal(message.NotificationType, notification.NotificationType);
    }

    [Fact(DisplayName = nameof(GivenADuplicateMessageThenShouldNotCreateDuplicatedMessage))]
    public async Task GivenADuplicateMessageThenShouldNotCreateDuplicatedMessage()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated);

        var notifications = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus)
            .ToListAsync(_fixture.CancellationToken);

        // Assert
        Assert.Single(notifications);
    }
}
