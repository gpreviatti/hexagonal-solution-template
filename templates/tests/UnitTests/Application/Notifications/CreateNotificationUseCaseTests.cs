using Application.Notifications;
using Domain.Notifications;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class CreateNotificationRequestValidationFixture
{
    public IValidator<CreateNotificationRequest> validator = new CreateNotificationRequestValidator();

    public CreateNotificationRequest GetValidRequest() =>
        new(Guid.NewGuid(), "TestNotification", "Success", "System", new { Test = "Message" });
}

public sealed class CreateNotificationRequestValidationTests(CreateNotificationRequestValidationFixture fixture) : IClassFixture<CreateNotificationRequestValidationFixture>
{
    private readonly CreateNotificationRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = _fixture.GetValidRequest();

        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = _fixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            NotificationType = string.Empty
        };

        // Act
        var result = await _fixture.validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("NotificationType");
    }
}

public sealed class CreateNotificationUseCaseFixture : BaseApplicationFixture<Notification, CreateNotificationRequest, CreateNotificationUseCase>
{
    public CreateNotificationUseCaseFixture()
    {
        MockServiceProviderServices();
        useCase = new(mockServiceProvider.Object);
    }

    public new void ClearInvocations()
    {
        base.ClearInvocations();
    }

    public CreateNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), "TestNotification", "Success", "System", new { Test = "Message" });

    public void VerifyFailedToCreateNotificationLog(int times = 1) =>
        mockLogger.VerifyLog(l => l.LogWarning("*Failed to create notification.*"), Times.Exactly(times));
}

public sealed class CreateNotificationUseCaseTests : IClassFixture<CreateNotificationUseCaseFixture>
{
    private readonly CreateNotificationUseCaseFixture _fixture;

    public CreateNotificationUseCaseTests(CreateNotificationUseCaseFixture fixture)
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
        _fixture.SetSuccessfulAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Success);
        Assert.Null(result.Message);
        Assert.NotNull(result.Data);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFailedToCreateNotificationLog(0);
        _fixture.VerifyAddAsync(1);
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
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyFailedToCreateNotificationLog(0);
        _fixture.VerifyAddAsync(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenFails))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenFails()
    {
        // Arrange
        var request = _fixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.SetFailedAddAsync();

        // Act
        var result = await _fixture.useCase.HandleAsync(request, _fixture.cancellationToken);

        // Assert
        Assert.False(result.Success);
        Assert.NotNull(result.Message);
        Assert.NotEmpty(result.Message);
        Assert.Equal("Failed to create notification.", result.Message);

        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFailedToCreateNotificationLog(1);
        _fixture.VerifyAddAsync(1);
    }
}
