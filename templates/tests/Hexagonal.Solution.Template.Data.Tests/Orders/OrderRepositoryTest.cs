using FluentAssertions;
using Hexagonal.Solution.Template.Data.Tests.Common;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;

[Collection("TestContainerSqlServerCollectionDefinition")]
public class OrderRepositoryTest : BaseOrderFixture , IClassFixture<TestContainerSqlServerFixture> 
{
    private readonly TestContainerSqlServerFixture _testContainerSqlServerFixture;

    public OrderRepositoryTest(TestContainerSqlServerFixture testContainerSqlServerFixture) : base(testContainerSqlServerFixture.myDbContext)
    {
        _testContainerSqlServerFixture = testContainerSqlServerFixture;
    }

    [Fact]
    public async Task Given_A_Id_Then_Return_Order_With_Success()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await repository.GetByIdAsync(id);


        // Assert
        result.Should().NotBeNull();
    }
}
