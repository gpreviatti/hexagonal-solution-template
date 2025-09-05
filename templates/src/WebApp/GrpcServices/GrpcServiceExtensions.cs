namespace WebApp.GrpcServices;

internal static class GrpcServiceExtensions
{
    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<OrderService>();

        return app;
    }
}