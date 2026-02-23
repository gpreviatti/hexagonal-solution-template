using Application.Common.Requests;
using Application.Common.UseCases;
using Application.Common.Helpers;
using Domain.Notifications;
using FluentValidation;

namespace Application.Notifications;

public sealed record CreateNotificationRequest(
    Guid CorrelationId,
    string NotificationType,
    string NotificationStatus,
    string? CreatedBy = null,
    object? Message = null
) : BaseRequest(CorrelationId);

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.NotificationType).NotEmpty();
    }
}

public sealed class CreateNotificationUseCase(IServiceProvider serviceProvider) : BaseInUseCase<CreateNotificationRequest>(serviceProvider)
{
    public override async Task HandleInternalAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = new Notification(
            request.NotificationType,
            request.NotificationStatus,
            request.Message,
            request.CreatedBy,
            request.TimezoneId
        );

        var addResult = await Repository.AddAsync(notification, request.CorrelationId, cancellationToken);

        if (addResult == 0)
        {
            Logs.FailedOperation(Logger, ClassName, HandleMethodName, request.CorrelationId, "Failed to create notification");
        }
    }
}
