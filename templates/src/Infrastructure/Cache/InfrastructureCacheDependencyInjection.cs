using Application.Common.Services;
using Infrastructure.Cache.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cache;

internal static class InfrastructureCacheDependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("RedisConnectionString");
        });

        services.AddHybridCache();

        services.AddSingleton<IHybridCacheService, HybridCacheService>();

        return services;
    }
}
