using Application.Notifications;
using Domain.Notifications;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class GetNotificationUseCaseFixture : BaseApplicationFixture<GetNotificationRequest, GetNotificationUseCase>
{
    public GetNotificationUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public GetNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), Math.Abs(AutoFixture.Create<int>()) + 1);
}

public sealed class GetNotificationUseCaseTests : IClassFixture<GetNotificationUseCaseFixture>
{
    private readonly GetNotificationUseCaseFixture _fixture;

    public GetNotificationUseCaseTests(GetNotificationUseCaseFixture fixture)
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
        var expectedNotification = _fixture.AutoFixture.Build<Notification>()
            .With(n => n.Id, request.Id)
            .Create();
        _fixture.MockRepository.SetupQueryable([expectedNotification]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedNotification.Id, result.Data.Id);
        Assert.Equal(expectedNotification.NotificationType, result.Data.NotificationType);
        Assert.Equal(expectedNotification.NotificationStatus, result.Data.NotificationStatus);

        _fixture.MockRepository.VerifyQueryable<Notification>();
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockRepository.VerifyQueryable<Notification>(0);
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNotificationNotFoundThenFails))]
    public async Task GivenAValidRequestWhenNotificationNotFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetupQueryable<Notification>([]);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Notification not found.", result.Message);

        _fixture.MockRepository.VerifyQueryable<Notification>();
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNotFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}
