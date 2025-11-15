using Application.Common.Constants;
using Application.Common.Messages;
using Application.Common.Repositories;
using Application.Common.Services;
using CommonTests.Fixtures;
using Domain.Notifications;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.WebApp.Messaging.Notifications;

public class CreateNotificationTestFixture : BaseFixture
{
    private readonly CustomWebApplicationFactory<Program> _customWebApplicationFactory;
    protected readonly IProduceService produceService;
    protected readonly IBaseRepository<Notification> notificationRepository;

    public CreateNotificationTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        _customWebApplicationFactory = customWebApplicationFactory;
        
        produceService = _customWebApplicationFactory.Services.GetRequiredService<IProduceService>();
        notificationRepository = _customWebApplicationFactory.Services.GetRequiredService<IBaseRepository<Notification>>();
    }

    public CreateNotificationMessage SetValidMessage() => autoFixture.Build<CreateNotificationMessage>().Create();

    public async Task HandleProducerAsync(CreateNotificationMessage message, int delay = 5000)
    {
        await produceService.HandleAsync(
            message, cancellationToken,
            NotificationType.OrderCreated
        );
        await Task.Delay(delay, cancellationToken);
    }
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateNotificationTest(CustomWebApplicationFactory<Program> customWebApplicationFactory) : CreateNotificationTestFixture(customWebApplicationFactory)
{
    [Fact(DisplayName = nameof(Given_A_Valid_Notification_Message_Then_Pass))]
    public async Task Given_A_Valid_Notification_Message_Then_Pass()
    {
        // Arrange
        var notificationMessage = SetValidMessage();

        // Act
        await HandleProducerAsync(notificationMessage);
        
        var notification = await notificationRepository.GetByWhereAsync(
            n => n.NotificationType == NotificationType.OrderCreated,
            cancellationToken
        );

        // Assert
        Assert.NotNull(notification);
    }
}