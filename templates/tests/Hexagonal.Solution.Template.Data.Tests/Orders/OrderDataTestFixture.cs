using AutoFixture;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;
public class OrderDataTestFixture : BaseFixture
{
    public MyDbContext myDbContext;
    public IOrderRepository repository;

    public OrderDataTestFixture(MyDbContext myDbContext)
    {
        this.myDbContext = myDbContext;

        Seed().Wait();

        this.repository = new OrderRepository(myDbContext);
    }

    public async Task Seed()
    {
        var items = autoFixture
            .Build<Item>()
            .Without(i => i.Id)
            .CreateMany(5)
            .ToList();

        var orders = autoFixture
            .Build<Order>()
        .Without(o => o.Id)
            .With(o => o.Items, items)
        .CreateMany(10);

        await myDbContext.AddRangeAsync(orders);

        await myDbContext.SaveChangesAsync();
    }
}
