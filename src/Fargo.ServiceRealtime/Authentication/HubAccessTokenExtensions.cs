using Fargo.RealtimeContracts;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Fargo.ServiceRealtime.Authentication;

/// <summary>
/// Lets hub clients pass the access token in the query string.
/// </summary>
public static class HubAccessTokenExtensions
{
    public const string AccessTokenQueryParameter = FargoRealtimeHub.AccessTokenQueryParameter;

    public const string HubsPathPrefix = "/hubs";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Reads the JWT bearer token from the <c>access_token</c> query parameter, for requests under
        /// <c>/hubs</c> only and when no <c>Authorization</c> header supplied one.
        /// </summary>
        /// <remarks>
        /// Browsers cannot set headers on a WebSocket or server-sent events request. The token is accepted
        /// nowhere else, to keep it out of other URLs; keep hub query strings out of proxy and access logs.
        /// It is validated like a header token, including the Fargo active-state and auth-version check.
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

    internal static string? ReadQueryToken(HttpRequest request)
    {
        var value = request.Query[AccessTokenQueryParameter].ToString();

        return string.IsNullOrWhiteSpace(value) ? null : value;
    }
}
