namespace WebApp.Endpoints;

internal static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapOrderEndpoints();
        return app;
    }
}