using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class GetAllOrdersTestFixture : BaseHttpFixture
{
    public static BasePaginatedRequest SetValidRequest() =>
        new(Guid.NewGuid(), 1, 10);

    public static BasePaginatedRequest SetInvalidPageRequest() =>
        new(Guid.NewGuid(), 0, 10);

    public static BasePaginatedRequest SetInvalidPageSizeRequest() =>
        new(Guid.NewGuid(), 1, 0);
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetAllOrdersTest : IClassFixture<GetAllOrdersTestFixture>
{
    private readonly GetAllOrdersTestFixture _fixture;
    public GetAllOrdersTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, GetAllOrdersTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders/paginated";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = GetAllOrdersTestFixture.SetValidRequest();

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
        var request = GetAllOrdersTestFixture.SetInvalidPageRequest();

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
        var request = GetAllOrdersTestFixture.SetInvalidPageSizeRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Contains("PageSize must be greater than 0", response.Message);
    }

    [Fact(DisplayName = nameof(GivenAnValidRequestWhenPassSearchByValuesFilterThenPass))]
    public async Task GivenAnValidRequestWhenPassSearchByValuesFilterThenPass()
    {
        // Arrange
        var request = new BasePaginatedRequest(
            Guid.NewGuid(), 1, 10,
            SearchByValues: new Dictionary<string, string> { { "Description", "client" } }
        );

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
}
