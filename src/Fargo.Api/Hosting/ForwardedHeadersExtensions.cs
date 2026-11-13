using Microsoft.AspNetCore.HttpOverrides;
using System.Net;

namespace Fargo.Api.Hosting;

/// <summary>
/// Forwarded-headers handling for running behind a reverse proxy or ingress.
/// </summary>
/// <remarks>
/// Without it the app sees the proxy's address as the client IP, which breaks HTTPS detection and collapses
/// the per-IP rate-limit partitions (login throttling, the global limiter) into one bucket for all clients.
/// Configured in the <c>ForwardedHeaders</c> section: <c>Enabled</c> (default off), <c>KnownProxies</c> and
/// <c>KnownNetworks</c> (CIDR, for example <c>10.0.0.0/8</c>). When enabled with neither list, the trusted
/// lists are cleared so headers are accepted from any peer. That is only safe when the app is reachable
/// exclusively through the proxy.
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
                // Clearing the defaults (loopback only) trusts any peer.
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
    /// Must run before HTTPS redirection, authentication and rate limiting so they see the real client IP and
    /// scheme. No-op unless enabled in configuration.
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
