using FluentAssertions;
using Hexagonal.Solution.Template.Data.Tests.Common;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;

[Collection("TestContainerSqlServerCollectionDefinition")]
public class OrderRepositoryTest(TestContainerSqlServerFixture fixture) : OrderDataTestFixture(fixture.myDbContext)
{
    [Fact]
    public async Task Given_A_Id_Then_Return_Order_With_Success()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await repository.GetByIdAsNoTrackingAsync(id, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(0);
    }
}
