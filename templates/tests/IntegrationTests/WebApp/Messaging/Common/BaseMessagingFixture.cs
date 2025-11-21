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
    public IProduceService produceService;
    public IBaseRepository repository;

    public void SetServices(AsyncServiceScope scope)
    {
        produceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
        repository = scope.ServiceProvider.GetRequiredService<IBaseRepository>();
    }

    public void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        produceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
        repository = scope.ServiceProvider.GetRequiredService<IBaseRepository>();
    }

    public async Task HandleProducerAsync<TMessage>(
        TMessage message,
        string queueName,
        int delay = 1500
    ) where TMessage : BaseMessage
    {
        await produceService.HandleAsync(message, cancellationToken, queueName);

        await Task.Delay(delay, cancellationToken);
    }
}