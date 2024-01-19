using Hexagonal.Solution.Template.Domain;
using Hexagonal.Solution.Template.Application;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Host.WebApp.Middlewares;
using Hexagonal.Solution.Template.Infrastructure.Log;

namespace Hexagonal.Solution.Template.Host.WebApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        ConfigureInternalServices(builder.Services, builder.Configuration);

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        app.Run();
    }

    public static void ConfigureInternalServices(IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddDomainServices()
            .AddApplicationServices()
            .AddInfrastructureLogServices()
            .AddInfrastructureDataServices(configuration);
    }
}