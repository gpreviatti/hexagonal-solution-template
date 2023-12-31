using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Application.Orders;
using Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
using System.Net;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Orders.Create;

[Collection("WebApplicationFactoryCollectionDefinition")]
public class CreateOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory) : CreateOrderTestFixture(customWebApplicationFactory)
{
    [Fact]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = SetValidRequest();

        // Act
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Success.Should().BeTrue();
        response.Data.Should().NotBeNull();
    }
}
