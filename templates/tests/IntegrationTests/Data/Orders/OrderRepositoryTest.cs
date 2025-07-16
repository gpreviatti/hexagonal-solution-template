using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.Data.Orders;

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class OrderRepositoryTest : IClassFixture<OrderDataTestFixture>
{
    private readonly OrderDataTestFixture? _fixture;
    public OrderRepositoryTest(
        CustomWebApplicationFactory<Program> factory,
        OrderDataTestFixture fixture
    )
    {
        _fixture = fixture;
        _fixture.SetRepository(factory);
    }

    [Fact]
    public async Task Given_A_Id_Then_Return_Order_With_Success()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _fixture!.Repository!.GetByIdAsNoTrackingAsync(id, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
    }
}
