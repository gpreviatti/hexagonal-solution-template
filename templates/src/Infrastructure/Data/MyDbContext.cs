using Infrastructure.Data.Orders.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Data;

public sealed class MyDbContext(
    DbContextOptions<MyDbContext> options
) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder
        .ApplyConfiguration(new OrderDbMapping())
        .ApplyConfiguration(new ItemDbMapping());

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        configurationBuilder
            .Properties<string>()
            .HaveColumnType("varchar")
            .HaveMaxLength(100);

        configurationBuilder
            .Properties<decimal>()
            .HavePrecision(18, 2);

        configurationBuilder
            .Properties<float>()
            .HavePrecision(18, 2);

        configurationBuilder
            .Properties<DateTime>()
            .HaveColumnType("datetime2");
    }
}
