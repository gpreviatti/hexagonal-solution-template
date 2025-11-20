using Domain.Orders;
using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.Data;

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class OrderRepositoryTest : IClassFixture<BaseDataFixture<Order>>
{
    private readonly BaseDataFixture<Order>? _fixture;
    public OrderRepositoryTest(CustomWebApplicationFactory<Program> factory, BaseDataFixture<Order> fixture)
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
        var result = await _fixture!.repository!.GetByIdAsNoTrackingAsync(
            id,
            _fixture.cancellationToken,
            o => o.Items
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_All_Orders_Paginated_With_Success()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_No_Orders_Paginated()
    {
        // Arrange
        var pageNumber = 50;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.True(totalRecords > 0);
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_Filtered_Orders_Paginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var valueToSearch = "client";
        var searchByValues = new Dictionary<string, string> {
            { "Description", valueToSearch }
        };

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken,
            searchByValues: searchByValues
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
        Assert.All(result, o => Assert.Contains(valueToSearch.ToLowerInvariant(), o.Description.ToLowerInvariant()));
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_No_Filtered_Orders_Paginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var searchByValues = new Dictionary<string, string> {
            { "Description", "non-existing-description" }
        };

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken,
            searchByValues: searchByValues
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.True(totalRecords > 0);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task Given_A_Valid_Request_Then_Return_Sorted_Orders_Paginated(bool sortDescending)
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var sortBy = "Description";

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken,
            sortBy: sortBy,
            sortDescending: sortDescending
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);

        var sortedResult = sortDescending
            ? [.. result.OrderByDescending(o => o.Description)]
            : result.OrderBy(o => o.Description).ToList();

        Assert.Equal(sortedResult, [.. result]);
    }
}
