using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace Fargo.ServiceHttp.Hosting;

/// <summary>
/// Configures forwarded-headers handling for running behind a reverse proxy or
/// ingress (TLS termination, load balancer).
/// </summary>
/// <remarks>
/// <para>
/// Without this, the app sees the proxy's address as the client IP and the
/// request scheme as the proxy-to-app scheme. That breaks HTTPS detection and —
/// critically — collapses the per-IP rate-limit partitions (login throttling,
/// the global limiter) into a single bucket for all clients.
/// </para>
/// <para>
/// Configuration (section <c>ForwardedHeaders</c>):
/// <list type="bullet">
///   <item><description><c>Enabled</c> — turn processing on (default: off; enable when deployed behind a proxy).</description></item>
///   <item><description><c>KnownProxies</c> — IP addresses of trusted proxies.</description></item>
///   <item><description><c>KnownNetworks</c> — trusted CIDR networks, e.g. <c>10.0.0.0/8</c>.</description></item>
/// </list>
/// When enabled but no known proxies/networks are configured, the trusted lists
/// are cleared so headers are accepted from any immediate peer. This is only
/// safe when the app is exclusively reachable through the proxy; otherwise set
/// <c>KnownProxies</c>/<c>KnownNetworks</c> to the real proxy addresses.
/// </para>
/// </remarks>
public static class ForwardedHeadersExtensions
{
    public const string SectionName = "ForwardedHeaders";

    public static IServiceCollection AddFargoForwardedHeaders(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(SectionName);

        if (!section.GetValue("Enabled", false))
        {
            return services;
        }

        var knownProxies = section.GetSection("KnownProxies").Get<string[]>() ?? [];
        var knownNetworks = section.GetSection("KnownNetworks").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            if (knownProxies.Length == 0 && knownNetworks.Length == 0)
            {
                // Trust the immediate peer only (the proxy in front of us).
                options.KnownProxies.Clear();
                options.KnownIPNetworks.Clear();
                return;
            }

            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();

            foreach (var proxy in knownProxies)
            {
                if (IPAddress.TryParse(proxy, out var ip))
                {
                    options.KnownProxies.Add(ip);
                }
            }

            foreach (var network in knownNetworks)
            {
                if (System.Net.IPNetwork.TryParse(network, out var net))
                {
                    options.KnownIPNetworks.Add(net);
                }
            }
        });

        return services;
    }

    /// <summary>
    /// Applies forwarded-headers processing. Must run before HTTPS redirection,
    /// authentication, and rate limiting so the real client IP and scheme are
    /// seen by everything downstream. No-op when not enabled in configuration.
    /// </summary>
    public static IApplicationBuilder UseFargoForwardedHeaders(
        this WebApplication app)
    {
        if (app.Configuration.GetSection(SectionName).GetValue("Enabled", false))
        {
            app.UseForwardedHeaders();
        }

        return app;
    }
}
