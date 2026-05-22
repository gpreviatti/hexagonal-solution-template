# Test Pattern — BaseInOutUseCase (GetAll Paginated)

```csharp
using Application.Common.Requests;
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class GetAllOrdersUseCaseFixture : BaseApplicationFixture<BasePaginatedRequest, GetAllOrdersUseCase>
{
    public GetAllOrdersUseCaseFixture() => UseCase = new(MockServiceProvider.Object);
    // Uses SetValidBasePaginatedRequest() inherited from BaseApplicationFixture
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class GetAllOrdersUseCaseTest : IClassFixture<GetAllOrdersUseCaseFixture>
{
    private readonly GetAllOrdersUseCaseFixture _fixture;

    public GetAllOrdersUseCaseTest(GetAllOrdersUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        var totalRecords = 5;
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrders = _fixture.AutoFixture.CreateMany<OrderDto>(totalRecords);
        _fixture.MockRepository.SetValidGetAllPaginatedAsyncNoIncludes<Order, OrderDto>(expectedOrders, totalRecords);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrders.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNoOrdersFoundThenFails))]
    public async Task GivenAValidRequestWhenNoOrdersFoundThenFails()
    {
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetInvalidGetAllPaginatedAsync<Order, OrderDto>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("No orders found.", result.Message);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(1);
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetFailedValidator(request);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockRepository.VerifyGetAllPaginatedNoIncludes<Order, OrderDto>(0);
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }
}
```
