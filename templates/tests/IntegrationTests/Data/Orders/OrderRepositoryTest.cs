using FluentAssertions;
using IntegrationTests.Data.Common;

namespace IntegrationTests.Data.Orders;

[Collection("DBContextCollectionDefinition")]
public sealed class OrderRepositoryTest(DbContextFixture fixture) : OrderDataTestFixture(fixture)
{
    [Fact]
    public async Task Given_A_Id_Then_Return_Order_With_Success()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await fixture!.orderRepository!.GetByIdAsNoTrackingAsync(id, cancellationToken);

        // Assert
        result.Should().NotBeNull();
        result!.Items.Count.Should().Be(0);
    }
}
