using Application.Common.Requests;
using Application.Orders;
using Domain.Orders;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Orders;

public sealed class GetAllOrdersUseCaseFixture : BaseApplicationFixture<Order, BasePaginatedRequest, GetAllOrdersUseCase>
{
    public GetAllOrdersUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public void VerifyNoOrdersFoundLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*No orders found.*"), Times.Exactly(times));
}

public sealed class GetAllOrdersUseCaseTest : IClassFixture<GetAllOrdersUseCaseFixture>
{
    private readonly GetAllOrdersUseCaseFixture _fixture;

    public GetAllOrdersUseCaseTest(GetAllOrdersUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var totalRecords = 5;
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrders = _fixture.autoFixture.CreateMany<Order>(totalRecords);

        _fixture.SetValidGetAllPaginatedAsyncNoIncludes(expectedOrders, totalRecords);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrders.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyGetAllPaginatedNoIncludes(1);
        _fixture.VerifyNoOrdersFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_When_No_Orders_Found_Then_Fails))]
    public async Task Given_A_Valid_Request_When_No_Orders_Found_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetInvalidGetAllPaginatedAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("No orders found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyGetAllPaginatedNoIncludes(1);
        _fixture.VerifyNoOrdersFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyGetAllPaginatedNoIncludes(0);
        _fixture.VerifyNoOrdersFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }
}
