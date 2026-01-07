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
            bool.TryParse(
                Environment.GetEnvironmentVariable("ENABLE_SENSITIVE_DATA_LOGGING"),
                out var enableSensitiveDataLogging
            );

            services.AddPooledDbContextFactory<MyDbContext>(options =>
            {
                options.UseSqlServer(
                    configuration.GetConnectionString("OrderDb") ?? throw new NullReferenceException("OrderDb connection string is not configured.")
                );
                options.EnableSensitiveDataLogging(enableSensitiveDataLogging);
            });

            services.AddScoped<IBaseRepository, BaseRepository>();

            return services;
        }
    }
}
