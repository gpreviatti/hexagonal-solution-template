﻿using Hexagonal.Solution.Template.Domain.Orders.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Domain;
public static class DomainDependencyInjection
{
    public static IServiceCollection AddDomainServices(this IServiceCollection services)
    {
        // Orders
        services.AddScoped<ICreateOrderService, CreateOrderService>();

        return services;
    }
}
