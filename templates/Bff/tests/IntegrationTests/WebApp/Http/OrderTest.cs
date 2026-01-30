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
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(GivenAGetByIdValidRequestThenPass))]
    public async Task GivenAGetByIdValidRequestThenPass()
    {
        // Arrange
        var id = 1;
        var url = string.Format(System.Globalization.CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() },
            { "CacheEnabled", "false" }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.NotNull(data);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
    }

    [Fact(DisplayName = nameof(GivenAGetByIdInvalidRequestThenFails))]
    public async Task GivenAGetByIdInvalidRequestThenFails()
    {
        // Arrange
        var id = 9999999;
        var url = string.Format(System.Globalization.CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() },
            { "CacheEnabled", "false" }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(result.IsSuccessStatusCode);
    }

    [Fact(DisplayName = nameof(GivenAValidCreateRequestThenPass))]
    public async Task GivenAValidCreateRequestThenPass()
    {
        // Arrange
        var url = string.Format(System.Globalization.CultureInfo.InvariantCulture, _fixture.ResourceUrl, string.Empty);
        CreateOrderRequest request = new(Guid.NewGuid(), "Test Order",
        [
            new("Item 1", "Description 1", 500.0m),
            new("Item 2", "Description 2", 500.0m)
        ]);

        // Act
        var result = await _fixture.ApiHelper.PostAsync(url, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.NotNull(data);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
    }
}
