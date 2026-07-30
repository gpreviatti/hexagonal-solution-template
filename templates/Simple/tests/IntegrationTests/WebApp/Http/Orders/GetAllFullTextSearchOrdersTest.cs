using System.Net;
using Core.Common.Requests;
using Core.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class GetAllFullTextSearchOrdersTestFixture : BaseHttpFixture
{
    public static BaseFullTextSearchPaginatedRequest SetValidRequest(string searchValue = "") => new(Guid.NewGuid(), 1, 10, SearchValue: searchValue);
    public static BaseFullTextSearchPaginatedRequest SetInvalidPageRequest() => new(Guid.NewGuid(), 0, 10);
    public static BaseFullTextSearchPaginatedRequest SetInvalidPageSizeRequest() => new(Guid.NewGuid(), 1, 0);
}

[Collection(nameof(WebApplicationFactoryCollectionDefinition))]
public class GetAllFullTextSearchOrdersTest : IClassFixture<GetAllFullTextSearchOrdersTestFixture>
{
    private readonly GetAllFullTextSearchOrdersTestFixture _fixture;

    public GetAllFullTextSearchOrdersTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, GetAllFullTextSearchOrdersTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/full-text-search-paginated";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersTestFixture.SetValidRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.TotalPages >= 0);
        Assert.True(response.TotalRecords >= 0);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidPageRequestThenFails))]
    public async Task GivenAnInvalidPageRequestThenFails()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersTestFixture.SetInvalidPageRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Contains("Page must be greater than 0", response.Message);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidPageSizeRequestThenFails))]
    public async Task GivenAnInvalidPageSizeRequestThenFails()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersTestFixture.SetInvalidPageSizeRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Contains("PageSize must be between 1 and 100", response.Message);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWithSearchValueThenPass))]
    public async Task GivenAValidRequestWithSearchValueThenPass()
    {
        // Arrange
        var request = GetAllFullTextSearchOrdersTestFixture.SetValidRequest(searchValue: "client");

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(response);
        Assert.True(
            result.StatusCode == HttpStatusCode.OK || result.StatusCode == HttpStatusCode.BadRequest,
            $"Unexpected status code: {result.StatusCode}"
        );
        if (result.StatusCode == HttpStatusCode.BadRequest)
            Assert.Equal("No orders found.", response.Message);
    }
}
