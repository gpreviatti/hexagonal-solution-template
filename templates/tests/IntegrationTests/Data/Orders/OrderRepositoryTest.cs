﻿using IntegrationTests.Common;
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
        var result = await _fixture!.Repository!.GetByIdAsNoTrackingAsync(
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
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync(
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
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync(
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
        var searchBy = "Description";
        var searchByValue = "xpto";

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken,
            searchBy: searchBy,
            searchByValue: searchByValue
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
        Assert.All(result, o => Assert.Contains(searchByValue.ToLowerInvariant(), o.Description.ToLowerInvariant()));
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_No_Filtered_Orders_Paginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var searchBy = "Description";
        var searchByValue = "non-existing-description";

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync(
            pageNumber,
            pageSize,
            _fixture.cancellationToken,
            searchBy: searchBy,
            searchByValue: searchByValue
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.Equal(0, totalRecords);
    }

    [Fact]
    public async Task Given_A_Valid_Request_Then_Return_Sorted_Orders_Paginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var sortBy = "Description";
        var sortDescending = true;

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync(
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

        var sortedResult = result.OrderByDescending(o => o.Description).ToList();
        Assert.Equal(sortedResult, [.. result]);
    }
}
