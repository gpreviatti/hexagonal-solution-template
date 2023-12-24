using Serilog;

namespace Hexagonal.Solution.Template.Application.Tests.Common;
public class BaseApplicationFixture : BaseFixture
{
    public Mock<ILogger> mockLogger = new();
   

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
