using AutoFixture;
using Bunit;
using Infrastructure.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace UnitTests.WebApp;

public class BaseComponentFixture : BunitContext
{
    public Mock<IBaseHttpService> HttpServiceMock { get; } = new();
    public Mock<ILoggerFactory> MockLoggerFactory { get; } = new();
    public Mock<ILogger> MockLogger { get; } = new();
    public CancellationToken CancellationToken { get; } = CancellationToken.None;
    public Fixture AutoFixture { get; }

    public BaseComponentFixture()
    {

        AutoFixture = new Fixture();
        AutoFixture.Behaviors.Add(new OmitOnRecursionBehavior());

        MockServices();
    }

    private void MockServices()
    {
        Services.AddSingleton(MockLoggerFactory.Object);
        MockLogger.Setup(l => l.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        MockLoggerFactory.Setup(x => x.CreateLogger(It.IsAny<string>())).Returns(MockLogger.Object);
    }

    public void ClearInvocations()
    {
        HttpServiceMock.Reset();
        MockLogger.Invocations.Clear();
    }

    public void SetupHttpServiceMock<TResponse>(string route, HttpMethod httpMethod, TResponse response) where TResponse : class => HttpServiceMock
        .Setup(x => x.SendAsync<TResponse>(route, httpMethod, CancellationToken, It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(response);

    public void SetupHttpServiceMock<TResponse>(TResponse response = null!) where TResponse : class => HttpServiceMock
        .Setup(x => x.SendAsync<TResponse>(It.IsAny<string>(), It.IsAny<HttpMethod>(), CancellationToken, It.IsAny<Dictionary<string, string>>()))
        .ReturnsAsync(response);
}
