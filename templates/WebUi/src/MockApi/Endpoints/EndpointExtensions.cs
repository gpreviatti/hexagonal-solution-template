namespace MockApi.Endpoints;

internal static class EndpointExtensions
{
    public static WebApplication MapEndpoints(this WebApplication app)
    {
        app.MapOrderSummaryEndpoints();
        return app;
    }
}