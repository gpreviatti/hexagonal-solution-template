using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Application.Common.UseCases;

public abstract class BaseUseCase
{
    protected IServiceProvider ServiceProvider { get; }
    protected ILogger Logger { get; }
    protected string ClassName { get; set; }

    protected BaseUseCase(IServiceProvider serviceProvider)
    {
        var classType = GetType();
        ClassName = classType.Name;

        ServiceProvider = serviceProvider;

        Logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger(classType);
    }
}