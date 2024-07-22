using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

namespace Infrastructure.Log;

public static class InfrastructureLogDependencyInjection
{
    public static IServiceCollection AddInfrastructureLogServices(this IServiceCollection services)
    {
        var logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .WriteTo.Console(theme: AnsiConsoleTheme.Code)
            .CreateLogger();

        services.AddSingleton<ILogger>(logger);

        return services;
    }
}
