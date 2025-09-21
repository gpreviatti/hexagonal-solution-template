using System.Net;
using Application.Common.Requests;
using Application.Orders;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class GetOrderTestFixture : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiHelper apiHelper;

    public string RESOURCE_URL = "orders/{0}";

    public GetOrderTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;

        apiHelper = new ApiHelper(this.customWebApplicationFactory.CreateClient());
    }
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory) : GetOrderTestFixture(customWebApplicationFactory)
{
    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var id = 1;
        var url = string.Format(RESOURCE_URL, id);
        apiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await apiHelper.GetAsync(url);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(data);
        Assert.Equal(id, data.Id);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
        Assert.NotNull(data.Description);
    }

    [Fact(DisplayName = nameof(Given_A_Invalid_Request_Then_Fails))]
    public async Task Given_A_Invalid_Request_Then_Fails()
    {
        // Arrange
        var id = 9999999;
        var url = string.Format(RESOURCE_URL, id);
        apiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await apiHelper.GetAsync(url);
        var response = await apiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
    }
}
