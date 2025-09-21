using System.Net;
using Application.Common.Requests;
using Application.Orders;
using AutoFixture;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class CreateOrderTestFixture : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiHelper apiHelper;

    public string RESOURCE_URL = "orders";

    public CreateOrderTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;

        apiHelper = new ApiHelper(this.customWebApplicationFactory.CreateClient());
    }

    public CreateOrderRequest SetValidRequest() => autoFixture.Create<CreateOrderRequest>();

    public CreateOrderRequest SetInvalidRequest() => autoFixture
            .Build<CreateOrderRequest>()
            .With(r => r.Description, string.Empty)
            .Create();
}

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
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
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
        Assert.NotNull(response);
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response.Success);
        Assert.Null(response.Data);
    }
}
