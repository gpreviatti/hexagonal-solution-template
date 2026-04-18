using Application.Common.Messages;
using Domain.Notifications;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Messaging.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;
using Microsoft.EntityFrameworkCore;
using Domain.Common.Enums;

namespace IntegrationTests.WebApp.Messaging.Notifications;

public class CreateNotificationTestFixture : BaseMessagingFixture
{
    public new void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        SetServices(scope);
    }

    public CreateNotificationMessage SetValidMessage() => AutoFixture.Build<CreateNotificationMessage>()
        .With(m => m.NotificationType, NotificationType.OrderCreated)
        .With(m => m.CreatedBy, $"CreatedBy-{Guid.NewGuid()}")
        .Create();
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
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());

        var notification = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType
                && n.NotificationStatus == message.NotificationStatus
                && n.CreatedBy == message.CreatedBy)
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
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());
        await _fixture.HandleProducerAsync(message, NotificationType.OrderCreated.ToString());

        var notifications = await _fixture.Repository.GetQueryable<Notification>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType
                && n.NotificationStatus == message.NotificationStatus
                && n.CreatedBy == message.CreatedBy)
            .ToListAsync(_fixture.CancellationToken);

        // Assert
        Assert.Single(notifications);
    }
}
