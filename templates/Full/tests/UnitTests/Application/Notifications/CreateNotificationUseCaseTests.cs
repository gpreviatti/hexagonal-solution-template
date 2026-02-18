using Application.Notifications;
using Domain.Notifications;
using FluentValidation;
using FluentValidation.TestHelper;
using Microsoft.Extensions.Logging;
using UnitTests.Application.Common;

namespace UnitTests.Application.Notifications;

public sealed class CreateNotificationRequestValidationFixture
{
    public IValidator<CreateNotificationRequest> Validator { get; } = new CreateNotificationRequestValidator();

    public static CreateNotificationRequest GetValidRequest() =>
        new(Guid.NewGuid(), "TestNotification", "Success", "System", new { Test = "Message" });
}

public sealed class CreateNotificationRequestValidationTests(CreateNotificationRequestValidationFixture fixture) : IClassFixture<CreateNotificationRequestValidationFixture>
{
    private readonly CreateNotificationRequestValidationFixture _fixture = fixture;

    [Fact(DisplayName = nameof(GivenAValidRequestThenPass))]
    public async Task GivenAValidRequestThenPass()
    {
        // Arrange
        var request = CreateNotificationRequestValidationFixture.GetValidRequest();

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateNotificationRequestValidationFixture.GetValidRequest() with
        {
            CorrelationId = Guid.Empty,
            NotificationType = string.Empty
        };

        // Act
        var result = await _fixture.Validator.TestValidateAsync(request);

        // Assert
        result.ShouldHaveValidationErrorFor("CorrelationId");
        result.ShouldHaveValidationErrorFor("NotificationType");
    }
}

public sealed class CreateNotificationUseCaseFixture : BaseApplicationFixture<CreateNotificationRequest, CreateNotificationUseCase>
{
    public CreateNotificationUseCaseFixture()
    {
        UseCase = new(MockServiceProvider.Object);
    }

    public static CreateNotificationRequest SetValidRequest() =>
        new(Guid.NewGuid(), "TestNotification", "Success", "System", new { Test = "Message" });

#pragma warning disable CA1848
    public void VerifyFailedToCreateNotificationLog(int times = 1) =>
        MockLogger.VerifyLog(l => l.LogWarning("*Failed to create notification.*"), Times.Exactly(times));
#pragma warning restore CA1848
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
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetSuccessfulAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFailedToCreateNotificationLog(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }

    [Fact(DisplayName = nameof(GivenAnInvalidRequestThenFails))]
    public async Task GivenAnInvalidRequestThenFails()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetFailedValidator(request);

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog(0);
        _fixture.VerifyFailedToCreateNotificationLog(0);
        _fixture.MockRepository.VerifyAddAsync<Notification>(0);
    }

    [Fact(DisplayName = nameof(GivenAValidRequestWhenRepositoryFailsThenFails))]
    public async Task GivenAValidRequestWhenRepositoryFailsThenFails()
    {
        // Arrange
        var request = CreateNotificationUseCaseFixture.SetValidRequest();
        _fixture.SetSuccessfulValidator(request);
        _fixture.MockRepository.SetFailedAddAsync<Notification>();

        // Act
        await _fixture.UseCase.HandleAsync(request, _fixture.CancellationToken);

        // Assert
        _fixture.VerifyStartUseCaseLog();
        _fixture.VerifyFinishUseCaseLog();
        _fixture.VerifyFailedToCreateNotificationLog(1);
        _fixture.MockRepository.VerifyAddAsync<Notification>(1);
    }
}
