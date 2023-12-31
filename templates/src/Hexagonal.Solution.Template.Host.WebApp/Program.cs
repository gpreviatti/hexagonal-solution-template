using Hexagonal.Solution.Template.Domain;
using Hexagonal.Solution.Template.Application;
using Hexagonal.Solution.Template.Infrastructure.Data;
using Hexagonal.Solution.Template.Host.WebApp.Middlewares;
using Hexagonal.Solution.Template.Infrastructure.Log;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services
    .AddDomainServices()
    .AddApplicationServices()
    .AddInfrastructureLogServices()
    .AddInfrastructureDataServices(builder.Configuration);

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

public partial class Program { }
