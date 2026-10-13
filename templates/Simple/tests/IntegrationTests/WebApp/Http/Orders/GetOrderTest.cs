using System.Globalization;
using System.Net;
using Core.Common.Requests;
using Core.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

[Collection(nameof(WebApplicationFactoryCollectionDefinition))]
public class GetOrderTest : IClassFixture<BaseHttpFixture>
{
    private readonly BaseHttpFixture _fixture;
    public GetOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, BaseHttpFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/{0}";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPassAsync))]
    public async Task GivenAValidRequestThenPassAsync()
    {
        // Arrange
        var id = 1;
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponseAsync<BaseResponse<OrderDto>>(result);
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

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFailsAsync))]
    public async Task GivenAInvalidRequestThenFailsAsync()
    {
        // Arrange
        var id = 9999999;
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, id);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>(StringComparer.Ordinal)
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        var result = await _fixture.ApiHelper.GetAsync(url);
        var response = await ApiHelper.DeSerializeResponseAsync<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Null(response.Data);
    }
}
