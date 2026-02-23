using Application.Common.Helpers;
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

            await ExecuteInternalAsync(scope.ServiceProvider, cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
                await Task.Delay(10000, cancellationToken);
        }
        catch (Exception ex)
        {
            Logs.Error(logger, nameof(BaseBackgroundService<>), nameof(ExecuteAsync), Guid.NewGuid(), ex.Message);

            throw;
        }
    }

    protected abstract Task ExecuteInternalAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken);
}