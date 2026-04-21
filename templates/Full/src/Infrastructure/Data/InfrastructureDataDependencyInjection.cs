using Application.Common.Repositories;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;

internal static class InfrastructureDataDependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddData(IConfiguration configuration)
        {
            var enableSensitiveDataLogging = bool.TryParse(
                Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING"),
                out var parsedValue
            ) && parsedValue;

            services.AddPooledDbContextFactory<MyDbContext>(options =>
            {
                options.UseNpgsql(
                    configuration.GetConnectionString("OrderDb") ?? throw new InvalidOperationException("OrderDb connection string is not configured.")
                );
                options.EnableSensitiveDataLogging(enableSensitiveDataLogging);
            });

            services.AddScoped<IBaseRepository, BaseRepository>();

            return services;
        }
    }
}
