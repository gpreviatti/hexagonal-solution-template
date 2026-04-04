using System.Globalization;
using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetOrderTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;
    public GetOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var id = 1;
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        var data = response?.Data;

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(data);
        Assert.Equal(id, data.Id);
        Assert.NotNull(data.Description);
        Assert.NotNull(data.PeriodSinceWasCreated);
        Assert.NotNull(data.Items);
        Assert.NotEmpty(data.Items);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var id = 9999999;
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
    }
}
