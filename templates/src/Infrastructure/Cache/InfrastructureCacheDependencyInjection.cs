using Application.Common.Services;
using Infrastructure.Cache.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Cache;

internal static class InfrastructureCacheDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddCache(IConfiguration configuration)
        {
            services
            .AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis") ?? throw new NullReferenceException("Redis connection string is not configured.");
                options.Configuration += ",abortConnect=false,connectTimeout=5000,syncTimeout=5000";
            })
            .AddHybridCache(options =>
            {
                options.DefaultEntryOptions = new()
                {
                    Expiration = TimeSpan.FromMinutes(30),
                    LocalCacheExpiration = TimeSpan.FromMinutes(30)
                };
            });

            services.AddSingleton<IHybridCacheService, HybridCacheService>();

            return services;
        }
    }
}
