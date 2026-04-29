using AutoFixture;
using Bunit;
using Infrastructure.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTests.WebApp;

public class BaseComponentFixture : TestContext
{
    public Mock<IBaseHttpService> HttpServiceMock { get; } = new();
    public Mock<ILogger> MockLogger { get; } = new();
    public CancellationToken CancellationToken { get; } = CancellationToken.None;
    public Fixture AutoFixture { get; }

    public BaseComponentFixture()
    {
        Services.AddSingleton(HttpServiceMock.Object);
        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    public void SetupHttpServiceMock<TResponse>(TResponse response = null!) where TResponse : class => HttpServiceMock
        .Setup(x => x.SendAsync<TResponse>(It.IsAny<string>(), It.IsAny<HttpMethod>(), CancellationToken, It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(response);

    public void SetupHttpServiceMock<TResponse>(string route, HttpMethod httpMethod, Dictionary<string, string> headers, TResponse response) where TResponse : class => HttpServiceMock
        .Setup(x => x.SendAsync<TResponse>(route, httpMethod, CancellationToken, headers))
        .ReturnsAsync(response);
}
