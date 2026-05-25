using Core.Common.Services;
using Infrastructure.Cache.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cache;

internal static class InfrastructureCacheDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCache()
        {
            services
            .AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new()
                {
                    Expiration = TimeSpan.FromHours(1),
                    LocalCacheExpiration = TimeSpan.FromHours(1)
                };
            });

            services.AddSingleton<IHybridCacheService, HybridCacheService>();

            return services;
        }
    }
}
