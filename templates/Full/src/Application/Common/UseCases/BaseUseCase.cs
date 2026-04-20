using System.Diagnostics;
using System.Diagnostics.Metrics;
using Application.Common.Helpers;
using Application.Common.Messages;
using Application.Common.Requests;
using Application.Common.Services;
using Domain.Common;
using Domain.Common.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public abstract class BaseUseCase
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected string ClassName { get; }
    protected ActivitySource ActivitySource { get; } = DefaultConfigurations.ActivitySource;
    protected IProduceService ProduceService { get; }
    protected Counter<int> UseCaseExecutedMetric { get; }
    protected Counter<int> UseCaseFailedMetric { get; }

    protected BaseUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        ServiceProvider = serviceProvider;

        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);

        ProduceService = serviceProvider.GetRequiredService<IProduceService>();

        UseCaseExecutedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{ClassName}.Executed", "total", "Number of times the use case was executed");

        UseCaseFailedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{ClassName}.Failed", "total", "Number of times the use case execution failed");
    }

    protected void HandleNotification(
        Guid correlationId,
        NotificationStatus notificationStatus,
        string createdBy,
        NotificationType notificationType,
        object message
    ) => _ = ProduceService.HandleAsync(
        new CreateNotificationMessage(
            correlationId,
            notificationType,
            notificationStatus,
            createdBy,
            message
        ),
        CancellationToken.None,
        queue: notificationType.ToString()
    );

    protected TResponse HandleFailedResponse<TRequest, TResponse>(
        TRequest request,
        Guid correlationId,
        NotificationType notificationType,
        string user = "System",
        string message = "Failed."
    )
    where TRequest : BaseRequest
    where TResponse : BaseResponse, new()
    {
        Logs.FailedOperation(Logger, correlationId, message);

        var response = Activator.CreateInstance<TResponse>();
        response.Success = false;
        response.Message = message;

        HandleNotification(correlationId, NotificationStatus.Failed, user, notificationType, response);

        UseCaseFailedMetric.Add(1);

        return response;
    }
}
