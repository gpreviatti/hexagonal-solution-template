using Application.Common.Services;
using Infrastructure.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cache;

internal static class InfrastructureCacheDependencyInjection
{
    public static IServiceCollection AddCache(this IServiceCollection services)
    {
        services.AddHybridCache();

        services.AddSingleton<IHybridCacheService, HybridCacheService>();

        return services;
    }
}
