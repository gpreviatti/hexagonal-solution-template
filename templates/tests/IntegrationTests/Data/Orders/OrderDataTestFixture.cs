using Application.Common.Repositories;
using CommonTests.Fixtures;
using Domain.Orders;
using IntegrationTests.Common;
using Microsoft.Extensions.DependencyInjection;
using WebApp;

namespace IntegrationTests.Data.Orders;
public class OrderDataTestFixture : BaseFixture
{
    public required IBaseRepository<Order> Repository;

    public void SetRepository(CustomWebApplicationFactory<Program> factory)
    {
        var scope = factory.Services.CreateAsyncScope();
        Repository = scope.ServiceProvider.GetRequiredService<IBaseRepository<Order>>();
    }
}
