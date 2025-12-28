using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.OpenTelemetry;
using Infrastructure.Cache;
using Infrastructure.Messaging;

namespace Infrastructure;

public static class InfrastructureDependencyInjection
{
    extension(WebApplicationBuilder builder)
    {
        public WebApplicationBuilder AddInfrastructure()
        {
            var configuration = builder.Configuration;

            builder.Services
                .AddData(configuration)
                .AddCache(configuration)
                .AddMessaging(configuration);

            builder.AddOpenTelemetry();

            return builder;
        }
    }
}
