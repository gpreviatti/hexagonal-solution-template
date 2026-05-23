using System.Threading.Channels;
using Core.Common.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Common;

internal abstract class BaseBackgroundChannelService<TService, TMessage>(IServiceScopeFactory serviceScopeFactory) : BackgroundService
{
    protected ILogger<BaseBackgroundChannelService<TService, TMessage>> logger;
    protected IConfiguration configuration;
    private readonly IServiceScopeFactory _serviceScopeFactory = serviceScopeFactory;

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger<BaseBackgroundChannelService<TService, TMessage>>();
            configuration = serviceProvider.GetRequiredService<IConfiguration>();

            var channel = serviceProvider.GetRequiredService<Channel<TMessage>>();

            while (await channel.Reader.WaitToReadAsync(cancellationToken))
            {
                var message = await channel.Reader.ReadAsync(cancellationToken);
                await ExecuteInternalAsync(serviceProvider, message, cancellationToken);
                await Task.Delay(1000, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            Logs.Error(logger, ex.Message);

            throw;
        }
    }

    protected abstract Task ExecuteInternalAsync(IServiceProvider serviceProvider, TMessage message, CancellationToken cancellationToken);
}
