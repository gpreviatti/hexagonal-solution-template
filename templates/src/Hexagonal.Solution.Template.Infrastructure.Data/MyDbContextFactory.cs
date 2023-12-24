using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System.Diagnostics.CodeAnalysis;

namespace Hexagonal.Solution.Template.Infrastructure.Data;
public class ContextFactory : IDesignTimeDbContextFactory<MyDbContext>
{
    [ExcludeFromCodeCoverage]
    public MyDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<MyDbContext>();

        builder.UseSqlServer("Server=127.0.0.1,1433;Database=HexagonalArchDb;User Id=sa;Password=yourStrong(!)Password;TrustServerCertificate=true;");

        return new MyDbContext(builder.Options);
    }
}
