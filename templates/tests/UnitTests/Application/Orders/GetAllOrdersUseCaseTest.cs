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

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public BasePaginatedRequest SetValidRequest() =>
        new(Guid.NewGuid(), 1, 10);

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

    [Fact]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedOrders = _fixture.autoFixture.CreateMany<Order>(5);
        var totalRecords = 20;

        _fixture.SetValidGetOrCreateAsync<(IEnumerable<Order>, int)>((expectedOrders, totalRecords));

        // Act
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedOrders.Count(), result.Data.Count());
        Assert.Equal(2, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyNoOrdersFoundLog(0);
        _fixture.VerifyCache<(IEnumerable<Order>, int)>(1);
    }

    [Fact]
    public async Task GivenAValidRequestWhenNoOrdersFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetInvalidGetOrCreateAsync<(IEnumerable<Order>, int)>();

        // Act
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("No orders found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyNoOrdersFoundLog(1);
        _fixture.VerifyCache<(IEnumerable<Order>, int)>(1);
    }

    [Fact]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.Handle(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyNoOrdersFoundLog(0);
        _fixture.VerifyCache<(IEnumerable<Order>, int)>(0);
    }
}