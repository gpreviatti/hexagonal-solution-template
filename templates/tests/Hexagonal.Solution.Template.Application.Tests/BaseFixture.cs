using Serilog;

namespace Hexagonal.Solution.Template.Application.Tests;
public class BaseFixture
{
    public Fixture autoFixture = new();

    public CancellationToken cancellationToken = CancellationToken.None;

    public Mock<ILogger> mockLogger = new();

    public BaseFixture()
    {
        autoFixture.Behaviors.Add(new OmitOnRecursionBehavior());
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
