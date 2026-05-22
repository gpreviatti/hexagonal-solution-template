using Infrastructure.Cache;
using Infrastructure.Data;
using Infrastructure.Messaging;
using Infrastructure.OpenTelemetry;
using Microsoft.AspNetCore.Builder;

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
