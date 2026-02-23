using Microsoft.Extensions.Logging;

namespace UnitTests.Application.Common;

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

    public static void VerifyStartOperation(this Mock<ILogger> mockLogger, int times = 1) =>
        VerifyInformation(mockLogger, $"Starting operation", times);

    public static void VerifyFinishOperation(this Mock<ILogger> mockLogger, int times = 1) =>
        VerifyInformation(mockLogger, $"Finished operation", times);

    public static void VerifyOperationFailed(this Mock<ILogger> mockLogger, int times = 1) =>
        VerifyWarning(mockLogger, "Failed operation", times);

    public static void VerifyNotFound(this Mock<ILogger> mockLogger, int times = 1) =>
        VerifyWarning(mockLogger, $"not found.", times);
}