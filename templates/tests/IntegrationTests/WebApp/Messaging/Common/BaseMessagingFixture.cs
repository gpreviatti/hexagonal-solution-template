using Application.Common.Services;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.WebApp.Messaging.Common;

public class BaseMessagingFixture : BaseFixture
{
    public IProduceService produceService;

    public void SetServices(AsyncServiceScope scope)
    {
        produceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
    }

    public void SetServices(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        produceService = scope.ServiceProvider.GetRequiredService<IProduceService>();
    }
}