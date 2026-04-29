using Microsoft.Extensions.Logging;
    
namespace UnitTests.Common;

internal static class LogMockExtensions
{
    public static void VerifyDebug(this Mock<ILogger> mockLogger, string expectedMessage, int times = 1) => mockLogger.Verify(
        logger => logger.Log(
            LogLevel.Debug,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
        ),
        Times.Exactly(times)
    );

    public static void VerifyInformation(this Mock<ILogger> mockLogger, string expectedMessage, int times = 1) => mockLogger.Verify(
        logger => logger.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
        ),
        Times.Exactly(times)
    );

    public static void VerifyError(this Mock<ILogger> mockLogger, string expectedMessage, int times = 1) => mockLogger.Verify(
        logger => logger.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
        ),
        Times.Exactly(times)
    );

    public static void VerifyWarning(this Mock<ILogger> mockLogger, string expectedMessage, int times = 1) => mockLogger.Verify(
        logger => logger.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
            It.IsAny<Exception>(),
            It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)
        ),
        Times.Exactly(times)
    );
}
