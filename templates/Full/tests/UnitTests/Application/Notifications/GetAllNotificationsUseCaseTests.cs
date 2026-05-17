using Application.Common.Requests;
using Application.Notifications;
using Domain.Notifications;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class GetAllNotificationsUseCaseFixture : BaseApplicationFixture<BasePaginatedRequest, GetAllNotificationsUseCase>
{
    public GetAllNotificationsUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public static new BasePaginatedRequest SetValidBasePaginatedRequest() =>
        new(Guid.NewGuid(), 1, 10);
}

public sealed class GetAllNotificationsUseCaseTests : IClassFixture<GetAllNotificationsUseCaseFixture>
{
    private readonly GetAllNotificationsUseCaseFixture _fixture;

    public GetAllNotificationsUseCaseTests(GetAllNotificationsUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var totalRecords = 5;
        var request = GetAllNotificationsUseCaseFixture.SetValidBasePaginatedRequest();
        var expectedNotifications = _fixture.AutoFixture.CreateMany<NotificationDto>(totalRecords);

        _fixture.MockRepository.SetValidGetAllPaginatedAsyncNoIncludes<Notification, NotificationDto>(expectedNotifications, totalRecords);

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedNotifications.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = GetAllNotificationsUseCaseFixture.SetValidBasePaginatedRequest();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(0);
        _fixture.MockLogger.VerifyFinishOperation(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNoNotificationsFoundThenFails))]
    public async Task GivenAValidRequestWhenNoNotificationsFoundThenFails()
    {
        // Arrange
        var request = GetAllNotificationsUseCaseFixture.SetValidBasePaginatedRequest();
        _fixture.MockRepository.SetInvalidGetAllPaginatedAsync<Notification, NotificationDto>();

        // Act
        var result = await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("No notifications found.", result.Message);

        _fixture.MockLogger.VerifyStartOperation();
        _fixture.MockLogger.VerifyNotFound(1);
        _fixture.MockLogger.VerifyFinishOperation();
    }
}
