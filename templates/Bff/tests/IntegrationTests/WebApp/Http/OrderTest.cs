using System.Net;
using Contracts.Common;
using Contracts.Orders;
using IntegrationTests.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http;

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class OrderTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;
    public OrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.resourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(Given_A_Get_By_Id_Valid_Request_Then_Pass))]
    public async Task Given_A_Get_By_Id_Valid_Request_Then_Pass()
    {
        // Arrange
        var id = 1;
        var url = string.Format(_fixture.resourceUrl, id);
        _fixture.apiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() },
            { "CacheEnabled", "false" }
        });

        // Act
        var result = await _fixture.apiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(data);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
    }

    [Fact(DisplayName = nameof(Given_A_Get_By_Id_Invalid_Request_Then_Fails))]
    public async Task Given_A_Get_By_Id_Invalid_Request_Then_Fails()
    {
        // Arrange
        var id = 9999999;
        var url = string.Format(_fixture.resourceUrl, id);
        _fixture.apiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() },
            { "CacheEnabled", "false" }
        });

        // Act
        var result = await _fixture.apiHelper.GetAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Create_Request_Then_Pass))]
    public async Task Given_A_Valid_Create_Request_Then_Pass()
    {
        // Arrange
        var url = string.Format(_fixture.resourceUrl, string.Empty);
        CreateOrderRequest request = new(Guid.NewGuid(), "Test Order",
        [
            new("Item 1", "Description 1", 500.0m),
            new("Item 2", "Description 2", 500.0m)
        ]);

        _fixture.apiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.apiHelper.PostAsync(url, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(data);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
    }
}
