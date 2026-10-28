using System.Globalization;
using System.Threading.RateLimiting;

namespace Fargo.ServiceHttp.RateLimiting;

/// <summary>
/// Registers the HTTP service's rate limiter, configured from the
/// <c>RateLimiting</c> settings section with safe built-in defaults.
/// </summary>
public static class FargoRateLimitingExtensions
{
    public const string GeneralAuthenticatedPolicy = "general-authenticated";
    public const string IdentityLoginPolicy = "identity-login";
    public const string IdentityRefreshPolicy = "identity-refresh";
    public const string IdentityDefaultPolicy = "identity-default";

    public static IServiceCollection AddFargoRateLimiter(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<RateLimitingOptions>()
            .Bind(configuration.GetSection(RateLimitingOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        // Resolve the configured options once at startup to build the limiters.
        var options = configuration
            .GetSection(RateLimitingOptions.SectionName)
            .Get<RateLimitingOptions>() ?? new RateLimitingOptions();

        services.AddRateLimiter(limiter =>
        {
            limiter.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Emit a Retry-After header so clients (including the Fargo CLI) know
            // how long to wait before retrying.
            limiter.OnRejected = (context, cancellationToken) =>
            {
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter =
                        ((int)Math.Ceiling(retryAfter.TotalSeconds))
                            .ToString(NumberFormatInfo.InvariantInfo);
                }

                return ValueTask.CompletedTask;
            };

            // When disabled, register the named policies as no-op limiters so the
            // RequireRateLimiting(...) calls on endpoints still resolve.
            if (!options.Enabled)
            {
                AddNoOpPolicy(limiter, GeneralAuthenticatedPolicy);
                AddNoOpPolicy(limiter, IdentityLoginPolicy);
                AddNoOpPolicy(limiter, IdentityRefreshPolicy);
                AddNoOpPolicy(limiter, IdentityDefaultPolicy);
                return;
            }

            // Global backstop, partitioned by client IP.
            limiter.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                context => FixedWindow(GetClientIp(context), options.Global));

            // General authenticated endpoints, partitioned by the authenticated
            // user, falling back to the client IP when the identity name is
            // unavailable.
            limiter.AddPolicy(GeneralAuthenticatedPolicy, context =>
                FixedWindow(
                    context.User.Identity?.Name ?? GetClientIp(context),
                    options.GeneralAuthenticated));

            limiter.AddPolicy(IdentityLoginPolicy, context =>
                FixedWindow(GetClientIp(context), options.IdentityLogin));

            limiter.AddPolicy(IdentityRefreshPolicy, context =>
                FixedWindow(GetClientIp(context), options.IdentityRefresh));

            limiter.AddPolicy(IdentityDefaultPolicy, context =>
                FixedWindow(GetClientIp(context), options.IdentityDefault));
        });

        return services;
    }

    private static RateLimitPartition<string> FixedWindow(string partitionKey, RateLimitPolicyOptions policy)
        => RateLimitPartition.GetFixedWindowLimiter(
            partitionKey,
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = policy.PermitLimit,
                Window = policy.Window,
                QueueLimit = policy.QueueLimit,
            });

    private static void AddNoOpPolicy(
        Microsoft.AspNetCore.RateLimiting.RateLimiterOptions limiter,
        string policyName)
        => limiter.AddPolicy(policyName, _ => RateLimitPartition.GetNoLimiter("disabled"));

    private static string GetClientIp(HttpContext context)
        => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
