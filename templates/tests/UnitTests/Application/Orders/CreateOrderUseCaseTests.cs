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
    public IValidator<CreateOrderRequest> validator = new CreateOrderRequestValidator();

    public CreateOrderRequest GetValidRequest() => new(Guid.NewGuid(), "new order", [
        new("item1", "description1", 10.0m),
        new("item2", "description2", 20.0m)
    ]);
}

public sealed class CreateOrderRequestValidationTests(CreateOrderRequestValidationFixture fixture) : IClassFixture<CreateOrderRequestValidationFixture>
{
    private readonly CreateOrderRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = _fixture.GetValidRequest();

        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            Description = string.Empty,
            Items = []
        };
        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

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
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public CreateOrderRequest SetValidRequest()
    {
        var items = autoFixture
            .CreateMany<CreateOrderItemRequest>(1);

        return new CreateOrderRequest(Guid.NewGuid(), "AwesomeComputer", [.. items]);
    }

    public static CreateOrderRequest SetInvalidRequestWithNoItems() =>
        new(Guid.NewGuid(), "AwesomeComputer", []);

    public void VerifyCreateOrderLogNoItemsError(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Order must have at least one item.*"), Times.Exactly(times));

    public void VerifyFailedToCreateOrderLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Failed to create order.*"), Times.Exactly(times));
}

public sealed class CreateOrderUseCaseTest : IClassFixture<CreateOrderUseCaseFixture>
{
    private readonly CreateOrderUseCaseFixture _fixture;

    public CreateOrderUseCaseTest(CreateOrderUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetSuccessfulAddAsync<Order>();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync<Order>(1);
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync<Order>(0);
        _fixture.VerifyProduce<CreateNotificationMessage>(0);
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails_When_There_Is_No_Items))]
    public async Task Given_A_Invalid_Request_Then_Fails_When_There_Is_No_Items()
    {
        // Arrange
        var request = CreateOrderUseCaseFixture.SetInvalidRequestWithNoItems();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(1);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync<Order>(0);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Fails_When_Repository_Returns_Zero))]
    public async Task Given_A_Valid_Request_Then_Fails_When_Repository_Returns_Zero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedAddAsync<Order>();

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyAddAsync<Order>(1);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(1);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyProduce<CreateNotificationMessage>();
    }
}
