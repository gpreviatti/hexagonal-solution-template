using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace Hexagonal.Solution.Template.Infrastructure.Data;

/// <summary>
/// This class is used to generate migrations. 
/// Change the database connection string to your local db connection to generate migrations
/// </summary>
[ExcludeFromCodeCoverage]
public class ContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    public MyDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MyDbContext>();

        builder.UseSqlServer("Server=127.0.0.1,1433;Database=HexagonalArchDb;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;");

        return new MyDbContext(builder.Options);
    }
}
