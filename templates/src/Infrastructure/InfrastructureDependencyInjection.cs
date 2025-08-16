﻿using Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Infrastructure.OpenTelemetry;
using Infrastructure.Cache;

namespace Infrastructure;
public static class InfrastructureDependencyInjection
{
    public static WebApplicationBuilder AddInfrastructure(this WebApplicationBuilder builder)
    {
        var configuration = builder.Configuration;

        builder.Services
            .AddData(configuration)
            .AddCache(configuration);

        builder.AddOpenTelemetry();

        return builder;
    }
}
