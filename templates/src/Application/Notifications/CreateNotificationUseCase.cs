using System.Diagnostics.Metrics;
using Application.Common.Constants;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed record CreateNotificationRequest(
    Guid CorrelationId,
    string NotificationType,
    string NotificationStatus,
    string CreatedBy = null,
    object Message = null
) : BaseRequest(CorrelationId);

public sealed class CreateNotificationRequestValidator : AbstractValidator<CreateNotificationRequest>
{
    public CreateNotificationRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.NotificationType).NotEmpty();
    }
}

public sealed class CreateNotificationUseCase(IServiceProvider serviceProvider)
    : BaseInOutUseCase<CreateNotificationRequest, BaseResponse<NotificationDto>, Notification, CreateNotificationUseCase>(
        serviceProvider,
        serviceProvider.GetService<IValidator<CreateNotificationRequest>>()
    )
{
    public static Counter<int> NotificationCreated = DefaultConfigurations.Meter
        .CreateCounter<int>("notification.created", "notifications", "Number of notifications created");

    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        CreateNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = new Notification(
            request.NotificationType,
            request.NotificationStatus,
            DateTime.UtcNow,
            request.CreatedBy,
            request.Message
        );

        var addResult = await _repository.AddAsync(notification, cancellationToken);

        if (addResult == 0)
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Failed to create notification.",
                ClassName,
                HandleMethodName,
                request.CorrelationId
            );
            return new(null, false, "Failed to create notification.");
        }

        NotificationCreated.Add(1);

        return new(new(
            notification.Id,
            notification.NotificationType,
            notification.NotificationStatus,
            notification.Message,
            notification.CreatedAt,
            notification.UpdatedAt,
            notification.CreatedBy,
            notification.UpdatedBy
        ), true);
    }
}
