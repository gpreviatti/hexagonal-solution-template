using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public abstract class BaseUseCase
{
    protected readonly IServiceProvider serviceProvider;
    protected readonly ILogger logger;
    protected readonly Stopwatch stopWatch = new();
    
    protected string ClassName;

    protected BaseUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        this.serviceProvider = serviceProvider;
        logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);
    }
}