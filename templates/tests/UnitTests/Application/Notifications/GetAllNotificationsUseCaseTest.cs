using Application.Common.Requests;
using Application.Notifications;
using Domain.Notifications;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class GetAllNotificationsUseCaseFixture : BaseApplicationFixture<Notification, BasePaginatedRequest, GetAllNotificationsUseCase>
{
    public GetAllNotificationsUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public new BasePaginatedRequest SetValidBasePaginatedRequest() => 
        new(Guid.NewGuid(), 1, 10);

    public void VerifyNoNotificationsFoundLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*No notifications found.*"), Times.Exactly(times));
}

public sealed class GetAllNotificationsUseCaseTest : IClassFixture<GetAllNotificationsUseCaseFixture>
{
    private readonly GetAllNotificationsUseCaseFixture _fixture;

    public GetAllNotificationsUseCaseTest(GetAllNotificationsUseCaseFixture fixture)
    {
        _fixture = fixture;
        _fixture.ClearInvocations();
    }

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var totalRecords = 5;
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        var expectedNotifications = _fixture.autoFixture.CreateMany<Notification>(totalRecords);

        _fixture.SetValidGetAllPaginatedAsyncNoIncludes(expectedNotifications, totalRecords);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Empty(result.Message);
        Assert.NotNull(result.Data);
        Assert.Equal(expectedNotifications.Count(), result.Data.Count());
        Assert.Equal(1, result.TotalPages);
        Assert.Equal(totalRecords, result.TotalRecords);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNoNotificationsFoundLog(0);
        _fixture.VerifyFinishUseCaseLog();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetFailedValidator(request);

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNoNotificationsFoundLog(0);
        _fixture.VerifyFinishUseCaseLog(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenNoNotificationsFoundThenFails))]
    public async Task GivenAValidRequestWhenNoNotificationsFoundThenFails()
    {
        // Arrange
        var request = _fixture.SetValidBasePaginatedRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetInvalidGetAllPaginatedAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotEmpty(result.Message);
        Assert.Equal("No notifications found.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyNoNotificationsFoundLog(1);
        _fixture.VerifyFinishUseCaseLog();
    }
}