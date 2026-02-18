using Application.Common.Messages;
using Application.Orders;
using Domain.Orders;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class CreateOrderRequestValidationFixture
{
    public IValidator<CreateOrderRequest> Validator { get; } = new CreateOrderRequestValidator();

    public static CreateOrderRequest GetValidRequest() => new(Guid.NewGuid(), "new order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture) : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = CreateOrderRequestValidationFixture.GetValidRequest();

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateOrderRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            Description = string.Empty,
            Items = []
        };
        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("Description");
        result.ShouldHaveValidationErrorFor("Items");
    }
}

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<CreateOrderRequest, CreateOrderUseCase>
{
    public CreateOrderUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture
            .CreateMany<CreateOrderItemRequest>(1);

        return new CreateOrderRequest(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);

#pragma warning disable CA1848
    public void VerifyCreateOrderLogNoItemsError(int times = 1) =>
        MockLogger.VerifyLog(l => l.LogWarning("*Order must have at least one item.*"), Times.Exactly(times));

    public void VerifyFailedToCreateOrderLog(int times = 1) =>
        MockLogger.VerifyLog(l => l.LogWarning("*Failed to create order.*"), Times.Exactly(times));
#pragma warning restore CA1848
}

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
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFailsWhenThereIsNoItems))]
    public async Task GivenAInvalidRequestThenFailsWhenThereIsNoItems()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(1);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.MockRepository.VerifyAddAsync<Order>(0);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenFailsWhenRepositoryReturnsZero))]
    public async Task GivenAValidRequestThenFailsWhenRepositoryReturnsZero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Order>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(
            request,
            _fixture.CancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.MockRepository.VerifyAddAsync<Order>(1);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(1);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }
}
