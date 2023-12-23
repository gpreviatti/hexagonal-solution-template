using AutoFixture;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;

public class BaseOrderFixture : BaseFixture 
{
    public readonly MyDbContext dbContext;

    public readonly IOrderRepository repository;

    public BaseOrderFixture(MyDbContext dbContext)
    {
        this.dbContext = dbContext;

        SeedOrder().Wait();

        repository = new OrderRepository(dbContext);
    }

    public async Task SeedOrder()
    {
        var orders = autoFixture.CreateMany<Order>(10);

        await dbContext.AddRangeAsync(orders);

        await dbContext.SaveChangesAsync();
    }

}
