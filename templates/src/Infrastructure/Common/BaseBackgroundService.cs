using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

internal abstract class BaseBackgroundService<TService>(
    ILogger<BaseBackgroundService<TService>> logger,
    IServiceScopeFactory serviceScopeFactory,
    IConfiguration configuration
) : BackgroundService
{
    protected readonly ILogger<BaseBackgroundService<TService>> logger = logger;
    protected readonly IConfiguration configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;

            while (!cancellationToken.IsCancellationRequested)
            {
                await ExecuteInternalAsync(serviceProvider, cancellationToken);

                await Task.Delay(5000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                logger.LogInformation("[BaseBackgroundService] | [ExecuteAsync] | Background service is stopping due to cancellation request.");
                return;
            }

            logger.LogError(
                "[BaseBackgroundService] | [ExecuteAsync] | Unexpected error in background service. | Message: {ErrorMessage} | StackTrace: {StackTrace}",
                ex.Message, ex.StackTrace
            );

            throw;
        }
    }

    protected abstract Task ExecuteInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}