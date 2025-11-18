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
    protected readonly IServiceProvider serviceProvider = serviceScopeFactory.CreateScope().ServiceProvider;
    protected readonly IConfiguration configuration = configuration;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await ExecuteInternalAsync(cancellationToken);

        while (!cancellationToken.IsCancellationRequested)
            await Task.Delay(10000, cancellationToken);
    }

    protected abstract Task ExecuteInternalAsync(CancellationToken cancellationToken);
}