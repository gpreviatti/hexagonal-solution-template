using System.Linq.Expressions;
using Application.Common.Constants;
using Application.Common.Repositories;
using Application.Common.Requests;
using Application.Common.UseCases;
using Domain.Notifications;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Notifications;

public sealed record GetNotificationRequest(Guid CorrelationId, int Id) : BaseRequest(CorrelationId);

public sealed class GetNotificationRequestValidator : AbstractValidator<GetNotificationRequest>
{
    public GetNotificationRequestValidator()
    {
        RuleFor(r => r.CorrelationId).NotEmpty();
        RuleFor(r => r.Id).GreaterThan(0);
    }
}

public sealed class GetNotificationUseCase(IServiceProvider serviceProvider) 
    : BaseInOutUseCase<GetNotificationRequest, BaseResponse<NotificationDto>>(serviceProvider)
{
    private readonly IBaseRepository<Notification> _repository = serviceProvider
        .GetRequiredService<IBaseRepository<Notification>>();
    public override async Task<BaseResponse<NotificationDto>> HandleInternalAsync(
        GetNotificationRequest request,
        CancellationToken cancellationToken
    )
    {
        var notification = await _repository.GetByIdAsNoTrackingAsync(
            request.Id,
            request.CorrelationId,
            n => new NotificationDto
            {
                Id = n.Id,
                Message = n.Message,
                NotificationType = n.NotificationType,
                NotificationStatus = n.NotificationStatus,
            },
            cancellationToken
        );

        if (notification is null)
        {
            logger.LogWarning(
                DefaultApplicationMessages.DefaultApplicationMessage + "Notification not found.",
                ClassName,
                HandleMethodName,
                request.CorrelationId
            );
            return new(false, null, "Notification not found.");
        }

        return new(true, notification);
    }
}
