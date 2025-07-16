using System.Net;
using Application.Common.Messages;
using Application.Orders;
using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.WebApp.Orders.Get;

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory) : GetOrderTestFixture(customWebApplicationFactory)
{
    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var id = 1;

        // Act
        var result = await apiHelper.GetAsync(RESOURCE_URL + "/" + id);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var id = 9999999;

        // Act
        var result = await apiHelper.GetAsync(RESOURCE_URL + "/" + id);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
    }
}
