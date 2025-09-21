﻿using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class CreateOrderUseCaseFixture : BaseApplicationFixture<Order, CreateOrderRequest, CreateOrderUseCase>
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
        _fixture.SetSuccessfulAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(1);
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
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(0);
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
        Assert.NotEmpty(result.Message);
        Assert.Equal("Order must have at least one item.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyCreateOrderLogNoItemsError(1);
        _fixture.VerifyFailedToCreateOrderLog(0);
        _fixture.VerifyAddAsync(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Fails_When_Repository_Returns_Zero))]
    public async Task Given_A_Valid_Request_Then_Fails_When_Repository_Returns_Zero()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(
            request,
            _fixture.cancellationToken
        );

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create order.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyAddAsync(1);
        _fixture.VerifyCreateOrderLogNoItemsError(0);
        _fixture.VerifyFailedToCreateOrderLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}
