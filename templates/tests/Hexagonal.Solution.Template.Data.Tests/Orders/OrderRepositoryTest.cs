using FluentAssertions;
using Hexagonal.Solution.Template.Data.Tests.Common;

namespace Hexagonal.Solution.Template.Data.Tests.Orders;

[Collection("TestContainerSqlServerCollectionDefinition")]
public class OrderRepositoryTest : IClassFixture<TestContainerSqlServerFixture>
{
    private readonly TestContainerSqlServerFixture _fixture;

    public OrderRepositoryTest(TestContainerSqlServerFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Given_A_Id_Then_Return_Order_With_Success()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _fixture.orderRepository.GetByIdAsync(id);


        // Assert
        result.Should().NotBeNull();
        result.Items.Should().BeNull();
    }
}
