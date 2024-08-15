using CommonTests.Fixtures;
using Serilog;

namespace UnitTests.Application.Common;
public class BaseApplicationFixture : BaseFixture
{
    public Mock<IServiceProvider> mockServiceProvider = new();

    public Mock<ILogger> mockLogger = new();

    public BaseApplicationFixture()
    {
        MockServiceProviderDefaultServices();
    }

    public void MockServiceProviderDefaultServices()
    {
        mockServiceProvider
            .Setup(r => r.GetService(typeof(ILogger)))
            .Returns(mockLogger.Object);
    }

    public void VerifyLoggerError<TLoggerObjectType>(int times)
    {
        mockLogger.Verify(
            l => l.Error(It.IsAny<string>(), It.IsAny<TLoggerObjectType>()),
            Times.Exactly(times)
        );
    }

    public void VerifyLoggerInformation<TLoggerObjectType>(int times)
    {
        mockLogger.Verify(
            l => l.Information(It.IsAny<string>(), It.IsAny<TLoggerObjectType>()),
            Times.Exactly(times)
        );
    }
}