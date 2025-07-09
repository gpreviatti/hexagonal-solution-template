using Application;
using Domain;
using Infrastructure.Common;
using WebApp.Middlewares;

namespace WebApp;

public sealed class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services
            .AddDomain()
            .AddApplication()
            .AddInfrastructure(builder.Configuration);

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
}
