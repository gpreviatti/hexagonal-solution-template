using System.Globalization;
using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class DeleteOrderTestFixture : BaseHttpFixture
{
    public async Task<OrderDto> CreateOrderAsync()
    {
        var createRequest = new CreateOrderRequest(
            Guid.NewGuid(),
            "Order created for delete integration test",
            [
                new("Desk", "Wood desk", 500m),
                new("Chair", "Office chair", 350m)
            ],
            "IntegrationTests",
            "UTC"
        );

        var createResult = await ApiHelper.PostAsync("orders", createRequest);
        var createResponse = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(createResult);

        Assert.Equal(HttpStatusCode.Created, createResult.StatusCode);
        Assert.NotNull(createResponse);
        Assert.True(createResponse!.Success);
        Assert.NotNull(createResponse.Data);

        return createResponse.Data!;
    }
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class DeleteOrderTest : IClassFixture<DeleteOrderTestFixture>
{
    private readonly DeleteOrderTestFixture _fixture;

    public DeleteOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, DeleteOrderTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var createdOrder = await _fixture.CreateOrderAsync();
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, createdOrder.Id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.DeleteAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(response);
        Assert.True(response!.Success);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, 9999999);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.DeleteAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.NotNull(response);
        Assert.False(response!.Success);
        Assert.Equal("Order not found.", response.Message);
    }
}
