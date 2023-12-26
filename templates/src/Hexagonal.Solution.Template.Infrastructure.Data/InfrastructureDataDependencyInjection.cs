﻿using Hexagonal.Solution.Template.Application.Orders;
using Hexagonal.Solution.Template.Infrastructure.Data.Orders.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Hexagonal.Solution.Template.Infrastructure.Data;
public static class InfrastructureDataDependencyInjection
{
    public static IServiceCollection AddInfrastructureDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDbContext<MyDbContext>(
            context => context.UseSqlServer(configuration.GetConnectionString("OrderDb"))
        );
        
        services.AddScoped<IOrderRepository, OrderRepository>();
        
        return services;
    }
}
