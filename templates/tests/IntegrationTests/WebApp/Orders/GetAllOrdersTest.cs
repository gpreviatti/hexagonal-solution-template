using System.Net;
using Application.Common.Requests;
using Application.Orders;
using CommonTests.Fixtures;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Common;
using WebApp;

namespace IntegrationTests.WebApp.Orders;

public class GetAllOrdersTestFixture : BaseFixture
{
    public CustomWebApplicationFactory<Program> customWebApplicationFactory;

    public ApiHelper apiHelper;

    public const string RESOURCE_URL = "orders/paginated";

    public GetAllOrdersTestFixture(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    {
        this.customWebApplicationFactory = customWebApplicationFactory;

        apiHelper = new ApiHelper(this.customWebApplicationFactory.CreateClient());
    }

    public BasePaginatedRequest SetValidRequest() =>
        new(Guid.NewGuid(), 1, 10);

    public BasePaginatedRequest SetInvalidPageRequest() =>
        new(Guid.NewGuid(), 0, 10);

    public BasePaginatedRequest SetInvalidPageSizeRequest() =>
        new(Guid.NewGuid(), 1, 0);
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public class GetAllOrdersTest(CustomWebApplicationFactory<Program> customWebApplicationFactory)
    : GetAllOrdersTestFixture(customWebApplicationFactory)
{
    [Fact(DisplayName = nameof(Given_A_Valid_Request_Then_Pass))]
    public async Task Given_A_Valid_Request_Then_Pass()
    {
        // Arrange
        var request = SetValidRequest();

        // Act
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

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
        var request = SetInvalidPageRequest();

        // Act
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

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
        var request = SetInvalidPageSizeRequest();

        // Act
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

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
        var result = await apiHelper.PostAsync(RESOURCE_URL, request);
        var response = await apiHelper.DeSerializeResponse<BasePaginatedResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
        Assert.True(response.TotalPages >= 0);
        Assert.True(response.TotalRecords >= 0);
    }
}
