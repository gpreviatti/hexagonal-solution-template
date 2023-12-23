using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Hexagonal.Solution.Template.Infrastructure.Data;
public class MyDbContext(
    DbContextOptions<MyDbContext> options
) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OrderDbMapping());
        modelBuilder.ApplyConfiguration(new ItemDbMapping());
    }
}
