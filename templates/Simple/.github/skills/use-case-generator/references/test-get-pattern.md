# Test Pattern — BaseInOutUseCase (Get by Id)

```csharp
using Application.Orders;
using Domain.Orders;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class GetOrderUseCaseFixture : BaseApplicationFixture<GetOrderRequest, GetOrderUseCase>
{
    public GetOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public GetOrderRequest SetValidRequest(int? id = null) =>
        new(Guid.NewGuid(), id ?? AutoFixture.Create<int>());
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class GetOrderUseCaseTest : IClassFixture<GetOrderUseCaseFixture>
{
    private readonly GetOrderUseCaseFixture _fixture;

    public GetOrderUseCaseTest(GetOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        var order = Order.Create("Test Order", [new("Item 1", "Desc 1", 500m)]).Value;
        var request = _fixture.SetValidRequest(order.Id);
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable(request.CorrelationId, null, [order]);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(order.Id, result.Data.Id);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        _fixture.MockRepository.VerifyQueryable<Order>(0);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenOrderNotFoundThenFails))]
    public async Task GivenAValidRequestWhenOrderNotFoundThenFails()
    {
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Order>(request.CorrelationId, null, []);

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("Order not found.", result.Message);
        _fixture.MockRepository.VerifyQueryable<Order>();
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }
}
```
