using Infrastructure.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace WebApp.Extensions;

internal static class RateLimitExtensions
{
    extension(IServiceCollection services)
    {
        internal IServiceCollection AddRateLimiting(IConfiguration configuration)
        {
            if (!configuration.GetValue<bool>("RATE_LIMITING_ENABLED"))
                return services;

            var serviceKeys = Enum.GetValues<ServicesKeys>();
            foreach (var serviceKey in serviceKeys)
            {
                var serviceConfig = configuration.GetSection("Http")
                    .GetChildren()
                    .FirstOrDefault(x => x["Name"] == serviceKey.ToString());

                if (serviceConfig != null && int.TryParse(serviceConfig["LimitPerMinute"], out int limitPerMinute))
                {
                    services.AddRateLimiter(options =>
                    {
                        options.AddFixedWindowLimiter(serviceKey.ToString(), limiterOptions =>
                        {
                            limiterOptions.PermitLimit = limitPerMinute;
                            limiterOptions.Window = TimeSpan.FromMinutes(1);
                        });

                        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
                        
                        options.OnRejected = async (context, cancellationToken) =>
                        {
                            context.HttpContext.Response.Headers.RetryAfter = "60 seconds";
                            await Task.CompletedTask;
                        };
                    });
                }
            }

            return services;
        }
    }
}
    