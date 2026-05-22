# Messaging Integration Test Template

```csharp
using Application.Common.Messages;
using Domain.Common.Enums;
using Domain.{DomainContext};
using IntegrationTests.Common;
using IntegrationTests.WebApp.Messaging.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.WebApp.Messaging.{Context};

public class {OperationName}TestFixture : BaseMessagingFixture
{
    public new void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        SetServices(scope);
    }

    public {MessageType} SetValidMessage() => AutoFixture.Build<{MessageType}>()
        .With(m => m.NotificationType, NotificationType.{QueueNotificationType})
        .Create();
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class {OperationName}Test : IClassFixture<{OperationName}TestFixture>
{
    private readonly {OperationName}TestFixture _fixture;

    public {OperationName}Test(CustomWebApplicationFactory<Program> factory, {OperationName}TestFixture fixture)
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
        await _fixture.HandleProducerAsync(message, NotificationType.{QueueNotificationType}.ToString());

        var entity = await _fixture.Repository.GetQueryable<{PersistedEntity}>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus)
            .FirstOrDefaultAsync(_fixture.CancellationToken);

        // Assert
        Assert.NotNull(entity);
        Assert.Equal(message.NotificationType, entity.NotificationType);
    }

    [Fact(DisplayName = nameof(GivenADuplicateMessageThenShouldNotCreateDuplicatedMessage))]
    public async Task GivenADuplicateMessageThenShouldNotCreateDuplicatedMessage()
    {
        // Arrange
        var message = _fixture.SetValidMessage();

        // Act
        await _fixture.HandleProducerAsync(message, NotificationType.{QueueNotificationType}.ToString());
        await _fixture.HandleProducerAsync(message, NotificationType.{QueueNotificationType}.ToString());

        var entities = await _fixture.Repository.GetQueryable<{PersistedEntity}>(Guid.NewGuid())
            .Where(n => n.NotificationType == message.NotificationType && n.NotificationStatus == message.NotificationStatus)
            .ToListAsync(_fixture.CancellationToken);

        // Assert
        Assert.Single(entities);
    }
}
```
