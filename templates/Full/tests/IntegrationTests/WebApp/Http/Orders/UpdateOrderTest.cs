using System.Globalization;
using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class UpdateOrderTestFixture : BaseHttpFixture
{
    public async Task<OrderDto> CreateOrderAsync()
    {
        var createRequest = new CreateOrderRequest(
            Guid.NewGuid(),
            "Order created for update integration test",
            [
                new("Keyboard", "Mechanical keyboard", 129.99m),
                new("Monitor", "27 inch monitor", 899.00m)
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

    public static UpdateOrderRequest SetValidRequest(int orderId) =>
        new(
            Guid.NewGuid(),
            orderId,
            "Updated order description",
            [
                new("Laptop", "Updated laptop", 2000m),
                new("Mouse", "Updated mouse", 150m)
            ],
            "IntegrationTests",
            "UTC"
        );
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class UpdateOrderTest : IClassFixture<UpdateOrderTestFixture>
{
    private readonly UpdateOrderTestFixture _fixture;

    public UpdateOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, UpdateOrderTestFixture fixture)
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
        var request = UpdateOrderTestFixture.SetValidRequest(createdOrder.Id);
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, createdOrder.Id);

        // Act
        var result = await _fixture.ApiHelper.PutAsync(url, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(response);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.Equal(createdOrder.Id, response.Data!.Id);
        Assert.Equal(request.Description, response.Data.Description);
        Assert.Equal(2150m, response.Data.Total);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = UpdateOrderTestFixture.SetValidRequest(9999999);
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, request.OrderId);

        // Act
        var result = await _fixture.ApiHelper.PutAsync(url, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.NotNull(response);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
        Assert.Equal("Order not found.", response.Message);
    }
}