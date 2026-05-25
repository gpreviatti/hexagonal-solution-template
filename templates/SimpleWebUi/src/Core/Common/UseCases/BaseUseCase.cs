using System.Diagnostics;
using System.Diagnostics.Metrics;
using Core.Common.Enums;
using Core.Common.Helpers;
using Core.Common.Messages;
using Core.Common.Requests;
using Core.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Core.Common.UseCases;

public abstract class BaseUseCase
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected string ClassName { get; }
    protected IProduceService ProducerService { get; }
    protected ActivitySource ActivitySource { get; } = DefaultConfigurations.ActivitySource;
    protected Counter<int> UseCaseExecutedMetric { get; }
    protected Counter<int> UseCaseFailedMetric { get; }

    protected BaseUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        ServiceProvider = serviceProvider;

        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);

        ProducerService = serviceProvider.GetRequiredService<IProduceService>();

        UseCaseExecutedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{ClassName}.Executed", "total", "Number of times the use case was executed");

        UseCaseFailedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{DefaultConfigurations.ApplicationName}.{ClassName}.Failed", "total", "Number of times the use case execution failed");
    }

    protected async Task HandleNotification(
        Guid correlationId,
        NotificationStatus notificationStatus,
        string createdBy,
        NotificationType notificationType,
        object message
    ) => await ProducerService.HandleAsync(
        new CreateNotificationMessage(
            correlationId,
            notificationType,
            notificationStatus,
            createdBy,
            message
        ),
        CancellationToken.None
    );

    protected async Task<TResponse> HandleFailedResponse<TResponse>(
        Guid correlationId,
        NotificationType notificationType,
        string user = "System",
        string message = "Failed."
    )
    where TResponse : BaseResponse, new()
    {
        Logs.FailedOperation(Logger, correlationId, message);

        var response = Activator.CreateInstance<TResponse>();
        response.Success = false;
        response.Message = message;

        await HandleNotification(correlationId, NotificationStatus.Failed, user, notificationType, response);

        UseCaseFailedMetric.Add(1);

        return response;
    }
}
