using System.Net;
using Application.Common.Requests;
using Application.Orders;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.Orders;

public class CreateOrderTestFixture : BaseHttpFixture
{
    public CreateOrderRequest SetValidRequest()
    {
        var items = AutoFixture.Build<CreateOrderItemRequest>()
            .With(i => i.Value, AutoFixture.Create<decimal>() + 1)
            .CreateMany(2)
            .ToArray();

        return AutoFixture.Build<CreateOrderRequest>()
            .With(r => r.Items, items)
            .With(r => r.TimezoneId, "UTC")
            .Create();
    }

    public CreateOrderRequest SetInvalidRequest() => AutoFixture
            .Build<CreateOrderRequest>()
            .With(r => r.Description, string.Empty)
            .With(r => r.TimezoneId, "UTC")
            .Create();
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class CreateOrderTest : IClassFixture<CreateOrderTestFixture>
{
    private readonly CreateOrderTestFixture _fixture;

    public CreateOrderTest(CustomWebApplicationFactory<Program> customWebApplicationFactory, CreateOrderTestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "orders";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        Assert.True(response!.Success);
        Assert.NotNull(response.Data);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetInvalidRequest();

        // Act
        var result = await _fixture.ApiHelper.PostAsync(_fixture.ResourceUrl, request);
        var response = await ApiHelper.DeSerializeResponse<BaseResponse<OrderDto>>(result);
        // Assert
        Assert.NotNull(response);
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        Assert.False(response.Success);
        Assert.Null(response.Data);
    }
}
