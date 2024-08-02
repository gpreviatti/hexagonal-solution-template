using Application.Orders;
using AutoFixture;
using Domain.Orders;
using Infrastructure.Data.Orders.Repositories;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace IntegrationTests.Data.Common;

public sealed  class DbContextFixture : IDisposable
{
    private MyDbContext? myDbContext;
    private readonly Fixture? autoFixture = new();

    public IOrderRepository? orderRepository;

    public DbContextFixture()
    {
        SetSqlServer().Wait();

        SetRepositories();
    }

    public async Task SetSqlServer()
    {
        var connectionString = "Server=127.0.0.1,1433;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;";

        var contextOptions = new DbContextOptionsBuilder<MyDbContext>()
                .UseSqlServer(connectionString)
                .Options;

        myDbContext = new(contextOptions);

        await MigrateAndSeed();
    }

    public async Task MigrateAndSeed()
    {
        await myDbContext!.Database.MigrateAsync();

        var items = autoFixture!
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

    public void SetRepositories() => orderRepository = new OrderRepository(myDbContext!);

    public void Dispose() => myDbContext!.Dispose();
}
