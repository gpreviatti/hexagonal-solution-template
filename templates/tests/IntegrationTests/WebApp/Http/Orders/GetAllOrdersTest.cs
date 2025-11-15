using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class GetAllOrdersTestFixture : BaseHttpFixture
{
    public BasePaginatedRequest SetValidRequest() =>
        new(Guid.NewGuid(), 1, 10);

    public BasePaginatedRequest SetInvalidPageRequest() =>
        new(Guid.NewGuid(), 0, 10);

    public BasePaginatedRequest SetInvalidPageSizeRequest() =>
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
        _fixture.resourceUrl = "orders/paginated";
    }

    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        // Act
        var result = await _fixture.apiHelper.PostAsync(_fixture.resourceUrl, request);
        var response = await _fixture.apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.TotalPages >= 0);
        Assert.True(response.TotalRecords >= 0);
    }

    [Fact(DisplayName = nameof(Given_An_Invalid_Page_Request_Then_Fails))]
    public async Task Given_An_Invalid_Page_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetInvalidPageRequest();

        // Act
        var result = await _fixture.apiHelper.PostAsync(_fixture.resourceUrl, request);
        var response = await _fixture.apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Contains("Page must be greater than 0", response.Message);
    }

    [Fact(DisplayName = nameof(Given_An_Invalid_PageSize_Request_Then_Fails))]
    public async Task Given_An_Invalid_PageSize_Request_Then_Fails()
    {
        // Arrange
        var request = _fixture.SetInvalidPageSizeRequest();

        // Act
        var result = await _fixture.apiHelper.PostAsync(_fixture.resourceUrl, request);
        var response = await _fixture.apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response!.Success);
        Assert.Contains("PageSize must be greater than 0", response.Message);
    }

    [Fact(DisplayName = nameof(Given_An_Valid_Request_When_Pass_Search_By_Values_Filter_Then_Pass))]
    public async Task Given_An_Valid_Request_When_Pass_Search_By_Values_Filter_Then_Pass()
    {
        // Arrange
        var request = new BasePaginatedRequest(
            Guid.NewGuid(), 1, 10,
            SearchByValues: new Dictionary<string, string> { { "Description", "client" } }
        );

        // Act
        var result = await _fixture.apiHelper.PostAsync(_fixture.resourceUrl, request);
        var response = await _fixture.apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.TotalPages >= 0);
        Assert.True(response.TotalRecords >= 0);
    }
}
