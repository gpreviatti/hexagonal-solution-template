using Application.Common.Requests;
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetAllFullTextSearchOrdersUseCaseFixture : BaseApplicationFixture<BaseFullTextSearchPaginatedRequest, GetAllFullTextSearchOrdersUseCase>
{
    public GetAllFullTextSearchOrdersUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public static BaseFullTextSearchPaginatedRequest SetValidRequest(string searchValue = "client") => new(Guid.NewGuid(), 1, 10, SearchValue: searchValue);

    public static BaseFullTextSearchPaginatedRequest SetInvalidPageRequest() => new(Guid.NewGuid(), 0, 10);
}

public sealed class GetAllFullTextSearchOrdersUseCaseTests : IClassFixture<GetAllFullTextSearchOrdersUseCaseFixture>
{
    private readonly GetAllFullTextSearchOrdersUseCaseFixture _fixture;

    public GetAllFullTextSearchOrdersUseCaseTests(GetAllFullTextSearchOrdersUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var totalRecords = 5;
        var request = GetAllFullTextSearchOrdersUseCaseFixture.SetValidRequest();
        var expectedOrders = _fixture.AutoFixture.CreateMany<OrderDto>(totalRecords);

        _fixture.MockRepository.SetValidGetAllFullTextSearchPaginatedAsync<Order, OrderDto>(expectedOrders, totalRecords);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrders.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllFullTextSearchPaginated<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNoOrdersFoundThenFails))]
    public async Task GivenAValidRequestWhenNoOrdersFoundThenFails()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersUseCaseFixture.SetValidRequest();
        _fixture.MockRepository.SetInvalidGetAllFullTextSearchPaginatedAsync<Order, OrderDto>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("No orders found.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllFullTextSearchPaginated<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidPageRequestThenFails))]
    public async Task GivenAnInvalidPageRequestThenFails()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersUseCaseFixture.SetInvalidPageRequest();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllFullTextSearchPaginated<Order, OrderDto>(0);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }
}
