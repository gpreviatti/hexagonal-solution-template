using CommonTests.Fixtures;
using IntegrationTests.Data.Common;

namespace IntegrationTests.Data.Orders;
public class OrderDataTestFixture(DbContextFixture fixture) : BaseFixture
{
    public DbContextFixture? fixture = fixture;
}
