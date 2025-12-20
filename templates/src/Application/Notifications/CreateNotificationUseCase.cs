using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.Logging;

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
            request.CreatedBy,
            request.Message
        );

        var addResult = await _repository.AddAsync(notification, request.CorrelationId, cancellationToken);

        if (addResult == 0)
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create notification.",
                ClassName,
                HandleMethodName,
                request.CorrelationId
            );
        }
    }
}
