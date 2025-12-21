using Application.Common.Repositories;
using Infrastructure.Data.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data;
internal static class InfrastructureDataDependencyInjection
{
    public static IServiceCollection AddData(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContextFactory<MyDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("OrderDb") ?? throw new NullReferenceException("OrderDb connection string is not configured."))
        );

        services.AddScoped<IBaseRepository, BaseRepository>();

        return services;
    }
}
