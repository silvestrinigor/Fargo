using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Fargo.ServiceRealtime.Authentication;

/// <summary>
/// Lets hub clients pass the access token in the query string.
/// </summary>
public static class HubAccessTokenExtensions
{
    /// <summary>
    /// The query parameter SignalR clients use for the access token.
    /// </summary>
    public const string AccessTokenQueryParameter = "access_token";

    /// <summary>
    /// The path prefix of every hub.
    /// </summary>
    public const string HubsPathPrefix = "/hubs";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Makes the JWT bearer handler read the token from the
        /// <c>access_token</c> query parameter, for requests under <c>/hubs</c>
        /// only, when no <c>Authorization</c> header is present.
        /// </summary>
        /// <remarks>
        /// Browsers cannot set headers on a WebSocket or server-sent events
        /// request, so SignalR clients send the token in the query string. It is
        /// accepted nowhere else, to keep tokens out of other URLs. Query strings
        /// may be recorded by proxies and access logs; keep them out of those
        /// logs for hub paths.
        /// The token is validated exactly like a header token, including the
        /// Fargo active-state and auth-version check.
        /// </remarks>
        public IServiceCollection AddHubAccessTokenFromQueryString()
        {
            // Post-configure runs after the Fargo JWT setup has set the events,
            // so this wraps them instead of replacing them.
            services.PostConfigure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.Events ??= new JwtBearerEvents();

                var previous = options.Events.OnMessageReceived;

                options.Events.OnMessageReceived = async context =>
                {
                    await previous(context);

                    if (context.Token is null &&
                        context.HttpContext.Request.Path.StartsWithSegments(HubsPathPrefix) &&
                        ReadQueryToken(context.HttpContext.Request) is { } token)
                    {
                        context.Token = token;
                    }
                };
            });

            return services;
        }
    }

    /// <summary>
    /// Reads the access token from the query string of a request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>The token, or <see langword="null"/> when there is none.</returns>
    internal static string? ReadQueryToken(HttpRequest request)
    {
        var value = request.Query[AccessTokenQueryParameter].ToString();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
