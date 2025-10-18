using Application.Common.Requests;
using Application.Notifications;
using Domain.Notifications;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class GetNotificationUseCaseFixture : BaseApplicationFixture<Notification, GetNotificationRequest, GetNotificationUseCase>
{
    public GetNotificationUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public GetNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), Math.Abs(autoFixture.Create<int>()) + 1);

    public void VerifyNotificationNotFoundLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Notification not found.*"), Times.Exactly(times));
}

public sealed class GetNotificationUseCaseTest : IClassFixture<GetNotificationUseCaseFixture>
{
    private readonly GetNotificationUseCaseFixture _fixture;

    public GetNotificationUseCaseTest(GetNotificationUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedNotification = _fixture.autoFixture.Create<Notification>();
        _fixture.SetupGetByIdAsNoTrackingAsync(expectedNotification);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedNotification.Id, result.Data.Id);
        Assert.Equal(expectedNotification.NotificationType, result.Data.NotificationType);
        Assert.Equal(expectedNotification.NotificationStatus, result.Data.NotificationStatus);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNotificationNotFoundThenFails))]
    public async Task GivenAValidRequestWhenNotificationNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Notification not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}
