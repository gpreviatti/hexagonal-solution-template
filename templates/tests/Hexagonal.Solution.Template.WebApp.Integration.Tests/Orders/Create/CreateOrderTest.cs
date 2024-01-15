using Hexagonal.Solution.Template.Application.Common.Messages;
using Hexagonal.Solution.Template.Application.Orders;
using Hexagonal.Solution.Template.Host.WebApp;
using Hexagonal.Solution.Template.WebApp.Integration.Tests.Common;
using System.Net;

namespace Hexagonal.Solution.Template.WebApp.Integration.Tests.Orders.Create;

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory) : CreateOrderTestFixture(customWebApplicationFactory)
{
    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
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

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var request = SetInvalidRequest();

        // Act
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        result.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Success.Should().BeFalse();
        response.Data.Should().BeNull();
    }
}
