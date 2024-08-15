using Application.Orders;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.Data.Orders;
public class OrderDataTestFixture : BaseFixture
{
    public required IOrderRepository Repository;

    public void SetRepository(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        Repository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
    }
}
