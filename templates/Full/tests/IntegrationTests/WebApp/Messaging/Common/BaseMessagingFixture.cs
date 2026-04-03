using Application.Common.Messages;
using Application.Common.Repositories;
using Application.Common.Services;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.WebApp.Messaging.Common;

public class BaseMessagingFixture : BaseFixture
{
    public IProduceService ProduceService { get; set; } = null!;
    public IBaseRepository Repository { get; set; } = null!;

    public void SetServices(AsyncServiceScope scope)
    {
        ProduceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
        Repository = scope.ServiceProvider.GetRequiredService<IBaseRepository>();
    }

    public void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        ProduceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
        Repository = scope.ServiceProvider.GetRequiredService<IBaseRepository>();
    }

    public async Task HandleProducerAsync<TMessage>(
        TMessage message,
        string queueName,
        int delay = 1500
    ) where TMessage : BaseMessage
    {
        await ProduceService.HandleAsync(message, CancellationToken, queueName);

        await Task.Delay(delay, CancellationToken);
    }
}
