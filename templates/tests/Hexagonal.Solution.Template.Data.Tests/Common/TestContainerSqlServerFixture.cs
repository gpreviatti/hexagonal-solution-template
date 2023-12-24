using AutoFixture;
using Hexagonal.Solution.Template.Application.Orders.Create;
using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Hexagonal.Solution.Template.Data.Tests.Common;

public class TestContainerSqlServerFixture : BaseFixture, IDisposable
{
    private readonly MsSqlContainer _sqlServerContainer;
    public string connectionString;
    public MyDbContext myDbContext;

    public IOrderRepository orderRepository;

    public TestContainerSqlServerFixture()
    {
        _sqlServerContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword(Guid.NewGuid().ToString())
            .WithCleanUp(true)
            .WithAutoRemove(true)
            .Build();

        _sqlServerContainer.StartAsync().Wait();

        connectionString = GetConnectionString();

        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        myDbContext = new MyDbContext(contextOptions);

        myDbContext.Database.Migrate();

        Seeds().Wait();

        SetRepositories();

    }

    private string GetConnectionString() =>
        _sqlServerContainer.GetConnectionString().Replace("localhost", "127.0.0.1");

    public void SetRepositories()
    {
        orderRepository = new OrderRepository(myDbContext);
    }

    public async Task Seeds()
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

    public void Dispose()
    {
        _sqlServerContainer.StopAsync().Wait();
        myDbContext.Dispose();
    }
}
