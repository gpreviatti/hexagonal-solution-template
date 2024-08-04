using Application.Common.Messages;
using Application.Orders;
using IntegrationTests.Common;
using System.Net;
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
        result.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var id = 100;

        // Act
        var result = await apiHelper.GetAsync(RESOURCE_URL + "/" + id);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
    }
}
