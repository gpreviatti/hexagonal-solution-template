# HTTP Integration Test Template

```csharp
using System.Globalization;
using System.Net;
using Application.Common.Requests;
using IntegrationTests.Common;
using IntegrationTests.WebApp.Http.Common;
using WebApp;

namespace IntegrationTests.WebApp.Http.{Context};

public class {OperationName}TestFixture : BaseHttpFixture
{
    // Keep only for POST/PUT scenarios
    public {RequestType} SetValidRequest() =>
        new(/* fill request with valid values */);

    // Keep only for POST/PUT scenarios
    public {RequestType} SetInvalidRequest() =>
        new(/* fill request with invalid values */);
}

[Collection("WebApplicationFactoryCollectionDefinition")]
public sealed class {OperationName}Test : IClassFixture<{OperationName}TestFixture>
{
    private readonly {OperationName}TestFixture _fixture;

    public {OperationName}Test(CustomWebApplicationFactory<Program> customWebApplicationFactory, {OperationName}TestFixture fixture)
    {
        _fixture = fixture;
        _fixture.SetApiHelper(customWebApplicationFactory);
        _fixture.ResourceUrl = "{resource}";
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        // For POST/PUT keep the request lines below; for GET/DELETE remove them.
        var request = _fixture.SetValidRequest();
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, /* optional route id */);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        // POST:  var result = await _fixture.ApiHelper.PostAsync(url, request);
        // PUT:   var result = await _fixture.ApiHelper.PutAsync(url, request);
        // GET:   var result = await _fixture.ApiHelper.GetAsync(url);
        // DELETE:var result = await _fixture.ApiHelper.DeleteAsync(url);
        var result = await _fixture.ApiHelper.{HttpCall};
        var response = await ApiHelper.DeSerializeResponse<{ResponseType}>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.{SuccessStatusCode}, result.StatusCode);
        Assert.NotNull(response);
        Assert.True(response!.Success);
    }

    [Fact(DisplayName = nameof(GivenAInvalidRequestThenFails))]
    public async Task GivenAInvalidRequestThenFails()
    {
        // Arrange
        // For POST/PUT keep the request lines below; for GET/DELETE remove them.
        var request = _fixture.SetInvalidRequest();
        var url = string.Format(CultureInfo.InvariantCulture, _fixture.ResourceUrl, /* optional route id */);
        _fixture.ApiHelper.AddHeaders(new Dictionary<string, string>
        {
            { "CorrelationId", Guid.NewGuid().ToString() }
        });

        // Act
        // POST:  var result = await _fixture.ApiHelper.PostAsync(url, request);
        // PUT:   var result = await _fixture.ApiHelper.PutAsync(url, request);
        // GET:   var result = await _fixture.ApiHelper.GetAsync(url);
        // DELETE:var result = await _fixture.ApiHelper.DeleteAsync(url);
        var result = await _fixture.ApiHelper.{HttpCall};
        var response = await ApiHelper.DeSerializeResponse<{ResponseType}>(result);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(HttpStatusCode.{FailureStatusCode}, result.StatusCode);
        Assert.NotNull(response);
        Assert.False(response!.Success);
    }
}
```
