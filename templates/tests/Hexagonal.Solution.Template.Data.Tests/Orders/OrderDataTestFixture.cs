using Hexagonal.Solution.Template.Application.Tests.Common;
using Hexagonal.Solution.Template.Data.Tests.Common;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;
public class OrderDataTestFixture(DbContextFixture fixture) : BaseFixture
{
    public DbContextFixture fixture = fixture;
}
