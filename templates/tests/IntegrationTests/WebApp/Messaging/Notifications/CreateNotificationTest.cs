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
    public IProduceService produceService;
    public IBaseRepository<Notification> notificationRepository;

    public void SetServiceProvider(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        produceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
        notificationRepository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Notification>>();
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
public sealed class CreateNotificationTest : IClassFixture<CreateNotificationTestFixture>
{
    private readonly CreateNotificationTestFixture _fixture;

    public CreateNotificationTest(CustomWebApplicationFactory<Program> factory, CreateNotificationTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetServiceProvider(factory);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Message_Then_Pass))]
    public async Task Given_A_Valid_Message_Then_Pass()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message);
        
        var notification = await _fixture.notificationRepository.GetByWhereAsync(
            n => n.NotificationType == NotificationType.OrderCreated,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(notification);
    }
}