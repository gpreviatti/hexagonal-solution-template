using System.Diagnostics;
using System.Diagnostics.Metrics;
using Domain.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public abstract class BaseUseCase
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected string ClassName { get; }
    protected ActivitySource ActivitySource { get; } = DefaultConfigurations.ActivitySource;
    protected Counter<int> UseCaseExecutedMetric { get; }
    protected Counter<int> UseCaseFailedMetric { get; }

    protected BaseUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        ServiceProvider = serviceProvider;

        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);

        UseCaseExecutedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{ClassName}.Executed", "total", "Number of times the use case was executed");

        UseCaseFailedMetric = DefaultConfigurations.Meter
            .CreateCounter<int>($"{ClassName}.Failed", "total", "Number of times the use case execution failed");
    }
}