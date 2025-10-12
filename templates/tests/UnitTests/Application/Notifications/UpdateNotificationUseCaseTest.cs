using Application.Notifications;
using Domain.Notifications;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class UpdateNotificationUseCaseFixture : BaseApplicationFixture<Notification, UpdateNotificationRequest, UpdateNotificationUseCase>
{
    public UpdateNotificationUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public UpdateNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), 1, "UpdatedNotification", "Error", "Updated message");

    public void VerifyNotificationNotFoundLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Notification not found.*"), Times.Exactly(times));

    public void VerifyFailedToUpdateNotificationLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Failed to update notification.*"), Times.Exactly(times));


}

public sealed class UpdateNotificationUseCaseTest : IClassFixture<UpdateNotificationUseCaseFixture>
{
    private readonly UpdateNotificationUseCaseFixture _fixture;

    public UpdateNotificationUseCaseTest(UpdateNotificationUseCaseFixture fixture)
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
        var existingNotification = _fixture.autoFixture.Create<Notification>();
        _fixture.SetupGetByIdAsNoTrackingAsync(existingNotification);
        _fixture.SetSuccessfulUpdate();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(request.NotificationType, result.Data.NotificationType);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(0);
        _fixture.VerifyFailedToUpdateNotificationLog(0);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyUpdate(1);
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
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(0);
        _fixture.VerifyFailedToUpdateNotificationLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyUpdate(0);
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
        Assert.NotEmpty(result.Message);
        Assert.Equal("Notification not found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(1);
        _fixture.VerifyFailedToUpdateNotificationLog(0);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyUpdate(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenFails))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        var existingNotification = _fixture.autoFixture.Create<Notification>();
        _fixture.SetupGetByIdAsNoTrackingAsync(existingNotification);
        _fixture.SetFailedUpdate();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to update notification.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotificationNotFoundLog(0);
        _fixture.VerifyFailedToUpdateNotificationLog(1);
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyUpdate(1);
    }
}
