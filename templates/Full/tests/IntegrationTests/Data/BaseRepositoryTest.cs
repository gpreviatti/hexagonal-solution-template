using Application.Orders;
using Domain.Orders;
using IntegrationTests.Common;
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
    public async Task GivenAIdThenReturnOrderWithSuccess()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _fixture!.repository!.GetByIdAsNoTrackingAsync<Order>(
            id,
            Guid.NewGuid(),
            _fixture.cancellationToken,
            includes: o => o.Items
        );

        // Assert
        Assert.NotNull(result);
        Assert.Equal(id, result!.Id);
        Assert.NotNull(result.Items);
        Assert.NotEmpty(result.Items);
    }

    [Fact]
    public async Task GivenAIdThenReturnOrderDtoWithSuccess()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await _fixture!.repository!.GetByIdAsNoTrackingAsync<Order, OrderDto>(
            id,
            Guid.NewGuid(),
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
            _fixture.cancellationToken
        );

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
        var orderTask = _fixture!.repository!.GetByIdAsNoTrackingAsync<Order>(
            id,
            Guid.NewGuid(),
            _fixture.cancellationToken
        );

        var notificationTask = _fixture!.repository!.GetByIdAsNoTrackingAsync<Domain.Notifications.Notification>(
            id,
            Guid.NewGuid(),
            _fixture.cancellationToken,
            newContext: true
        );

        await Task.WhenAll(orderTask, notificationTask);
        var order = await orderTask;
        var notification = await notificationTask;

        // Assert
        Assert.NotNull(order);
        Assert.Equal(id, order!.Id);

        Assert.NotNull(notification);
        Assert.Equal(id, notification!.Id);
        Assert.NotNull(notification.NotificationType);
        Assert.NotNull(notification.NotificationStatus);
    }

    [Fact]
    public async Task GivenAValidRequestThenReturnAllOrdersPaginatedWithSuccess()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
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
    public async Task GivenAValidRequestThenReturnAllOrdersDtosPaginatedWithSuccess()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order, OrderDto>(
            Guid.NewGuid(),
            pageNumber,
            pageSize,
            selector: o => new OrderDto() {
                Id = o.Id,
                Total = o.Total,
                Items = o.Items.Select(i => new ItemDto
                {
                    Id = i.Id,
                    Name = i.Name,
                    Value = i.Value
                }).ToArray()
            },
            _fixture.cancellationToken
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
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
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
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
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
    public async Task GivenAValidRequestThenReturnNoFilteredOrdersPaginated()
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var searchByValues = new Dictionary<string, string> {
            { "Description", "non-existing-description" }
        };

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
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
    public async Task GivenAValidRequestThenReturnSortedOrdersPaginated(bool sortDescending)
    {
        // Arrange
        var pageNumber = 1;
        var pageSize = 5;
        var sortBy = "Description";

        // Act
        var (result, totalRecords) = await _fixture!.repository!.GetAllPaginatedAsync<Order>(
            Guid.NewGuid(),
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
