using System.Net;
using Application.Common.Messages;
using SerilogTimings.Extensions;
using ILogger = Serilog.ILogger;

namespace WebApp.Middlewares;

internal sealed class ExceptionHandlingMiddleware(RequestDelegate next, ILogger logger)
{
    private readonly RequestDelegate _next = next;
    private readonly ILogger _logger = logger;

    public async Task InvokeAsync(HttpContext context)
    {
        using (_logger.TimeOperation("Request was executed", context.Request.Path))
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
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        _logger.Error(exception, exception.Message);

        var response = new BaseResponse
        {
            Message = exception.Message
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
        await context.Response.WriteAsJsonAsync(response);
    }
}

public record ExceptionResponse(HttpStatusCode StatusCode, string Description);
