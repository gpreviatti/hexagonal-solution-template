using AutoFixture;
using Hexagonal.Solution.Template.Application.Orders;
using Hexagonal.Solution.Template.Domain.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;
using Microsoft.EntityFrameworkCore;
using Testcontainers.MsSql;

namespace Hexagonal.Solution.Template.Data.Tests.Common;

public class DbContextFixture : IDisposable
{
    private MsSqlContainer _sqlServerContainer;
    private MyDbContext myDbContext;
    private readonly Fixture autoFixture = new();

    public IOrderRepository orderRepository;

    public DbContextFixture()
    {
        SetSqlServerTestContainer().Wait();

        SetRepositories();
    }

    public async Task SetSqlServerTestContainer()
    {
        _sqlServerContainer = new MsSqlBuilder()
           .WithName("sql-server-2022-data-tests")
           .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
           .WithPortBinding(1433, 1433)
           .WithPassword(Guid.NewGuid().ToString())
           .WithCleanUp(true)
           .WithAutoRemove(true)
           .Build();

        await _sqlServerContainer.StartAsync();

        var connectionString = _sqlServerContainer.GetConnectionString().Replace("localhost", "127.0.0.1");

        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        myDbContext = new(contextOptions);

        await MigrateAndSeed();
    }

    public async Task MigrateAndSeed()
    {
        await myDbContext.Database.MigrateAsync();

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

    public void SetRepositories()
    {
        orderRepository = new OrderRepository(myDbContext);
    }

    public void Dispose()
    {
        _sqlServerContainer.StopAsync().Wait();
        myDbContext.Dispose();
    }
}
