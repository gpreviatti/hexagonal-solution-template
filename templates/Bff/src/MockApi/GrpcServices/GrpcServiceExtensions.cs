namespace MockApi.GrpcServices;

internal static class GrpcServiceExtensions
{
    public static WebApplication MapGrpcServices(this WebApplication app)
    {
        app.MapGrpcService<PaymentService>();

        return app;
    }
}