# Test Pattern — BaseInOutUseCase (Create)

```csharp
using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using FluentValidation;
using FluentValidation.TestHelper;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

// ── 1. Validator Fixture ────────────────────────────────────────────────────

public sealed class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> Validator { get; } = new CreateOrderRequestValidator();

    public static CreateOrderRequest GetValidRequest() => new(Guid.NewGuid(), "new order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

// ── 2. Validator Tests ──────────────────────────────────────────────────────

public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture)
    : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        var request = CreateOrderRequestValidationFixture.GetValidRequest();
        var result = await _fixture.Validator.TestValidateAsync(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        var request = CreateOrderRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            Description = string.Empty,
            Items = []
        };
        var result = await _fixture.Validator.TestValidateAsync(request);
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("Description");
        result.ShouldHaveValidationErrorFor("Items");
    }
}

// ── 3. Use Case Fixture ─────────────────────────────────────────────────────

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<CreateOrderRequest, CreateOrderUseCase>
{
    public CreateOrderUseCaseFixture() => UseCase = new(MockServiceProvider.Object);

    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture.CreateMany<CreateOrderItemRequest>(1);
        return new(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);
}

// ── 4. Use Case Tests ───────────────────────────────────────────────────────

public sealed class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
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
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation(0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryReturnsZeroThenFails))]
    public async Task GivenAValidRequestWhenRepositoryReturnsZeroThenFails()
    {
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Order>();

        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyFinishOperation();
        _fixture.MockLogger.VerifyWarning("Failed to create order.", 1);
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }
}
```
