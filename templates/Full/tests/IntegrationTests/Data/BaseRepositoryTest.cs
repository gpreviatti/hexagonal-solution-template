using Application.Orders;
using Domain.Orders;
using IntegrationTests.Common;
using Microsoft.EntityFrameworkCore;
using WebApp;

namespace IntegrationTests.Data;

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class BaseRepositoryTest : IClassFixture<BaseDataFixture>
{
    private readonly BaseDataFixture? _fixture;
    public BaseRepositoryTest(CustomWebApplicationFactory<Program> factory, BaseDataFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetRepository(factory);
    }

    [Fact]
    public async Task GivenAIdThenReturnOrderDtoWithSuccess()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _fixture!.Repository!.GetQueryable<Order>(Guid.NewGuid())
            .Where(o => o.Id == id)
            .Select(o => new OrderDto()
            {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            }).FirstOrDefaultAsync(_fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GivenAOrderAndNotificationShouldExecuteInParallelWithSuccess()
    {
        // Arrange
        var id = 1;

        // Act
        var orderTask1 = _fixture!.Repository!.GetQueryable<Order>(Guid.NewGuid())
            .Where(o => o.Id == id)
            .Select(o => new OrderDto()
            {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            }).FirstOrDefaultAsync(_fixture.CancellationToken);

        var orderTask2 = _fixture!.Repository!.GetQueryable<Order>(Guid.NewGuid(), true)
            .Where(n => n.Id == id)
            .Select(o => new OrderDto()
            {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            })
            .FirstOrDefaultAsync(_fixture.CancellationToken);

        await Task.WhenAll(orderTask1, orderTask2);
        var order1 = await orderTask1;
        var order2 = await orderTask2;

        // Assert
        Assert.NotNull(order1);
        Assert.Equal(id, order1!.Id);
        Assert.NotNull(order2);
        Assert.Equal(id, order2!.Id);
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnAllOrdersPaginatedWithSuccess()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            _fixture.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnAllOrdersDtosPaginatedWithSuccess()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order, OrderDto>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            selector: o => new OrderDto()
            {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            },
            _fixture.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
        Assert.All(result, r => Assert.IsType<OrderDto>(r));
        Assert.All(result, r => Assert.NotNull(r.Items));
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnNoOrdersPaginated()
    {
        // Arrange
        var pageNumber = 50;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            _fixture.CancellationToken
        );

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
        Assert.True(totalRecords > 0);
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnFilteredOrdersPaginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var valueToSearch = "client";
        var searchByValues = new Dictionary<string, string> {
            { "Description", valueToSearch }
        };

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            _fixture.CancellationToken,
            searchByValues: searchByValues
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
        Assert.All(result, o => Assert.Contains(valueToSearch.ToLowerInvariant(), o.Description.ToLowerInvariant()));
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnNoFilteredOrdersPaginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var searchByValues = new Dictionary<string, string> {
            { "Description", "non-existing-description" }
        };

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            _fixture.CancellationToken,
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
    public async Task GivenAValidRequestThenReturnSortedOrdersPaginated(bool sortDescending)
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var sortBy = "Description";

        // Act
        var (result, totalRecords) = await _fixture!.Repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            _fixture.CancellationToken,
            sortBy: sortBy,
            sortDescending: sortDescending
        );

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
        Assert.True(totalRecords > 0);
        Assert.All(result, r => Assert.NotNull(r.Description));
    }
}
