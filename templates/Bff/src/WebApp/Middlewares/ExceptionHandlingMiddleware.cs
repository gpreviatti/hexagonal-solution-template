using System.Net;

namespace WebApp.Middlewares;

internal sealed partial class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger = logger;
    private readonly string _className = nameof(ExceptionHandlingMiddleware);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int) HttpStatusCode.BadRequest;

        RequestFailedLog(_logger, _className, nameof(HandleExceptionAsync), exception.Message);
    }

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "[{ClassName}] | [{Method}] | Request failed | Message: {Message}"
    )]
    public static partial void RequestFailedLog(ILogger logger, string className, string method, string message);
}

public record ExceptionResponse(HttpStatusCode StatusCode, string Description);
