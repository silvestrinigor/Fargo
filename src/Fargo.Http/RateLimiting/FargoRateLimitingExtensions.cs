using System.Threading.RateLimiting;

public static class FargoRateLimitingExtensions
{
    public static IServiceCollection AddFargoRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetClientIp(context),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 100,
                            Window = TimeSpan.FromMinutes(1),
                            QueueLimit = 0
                        }));

            options.AddPolicy("general-authenticated", context =>
            {
                var identity = context.User.Identity?.Name;

                return RateLimitPartition.GetFixedWindowLimiter(
                    identity ?? "anonymous",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    });
            });

            options.AddPolicy("identity-login", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("identity-refresh", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 30,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));

            options.AddPolicy("identity-default", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    GetClientIp(context),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        return services;
    }

    private static string GetClientIp(HttpContext context)
    {
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
}
