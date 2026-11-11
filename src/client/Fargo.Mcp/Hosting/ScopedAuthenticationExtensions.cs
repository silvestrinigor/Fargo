using Fargo.ClientHttp.Authentication;
using Fargo.Mcp.Authentication;
using Fargo.Mcp.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Kiota.Abstractions.Authentication;

namespace Fargo.Mcp.Hosting;

internal static class ScopedAuthenticationExtensions
{
    /// <summary>
    /// Replaces the singleton token plumbing registered by
    /// <c>AddFargoHttpClient</c> with scoped equivalents, and builds the
    /// <see cref="FargoMcpOptions"/> of each scope from the bearer token of the
    /// current HTTP request. This guarantees one caller's token is never visible
    /// to another caller. Call after <c>AddFargoHttpClient</c> and
    /// <c>AddHttpContextAccessor</c>.
    /// </summary>
    public static IServiceCollection AddScopedFargoAuthentication(
        this IServiceCollection services,
        FargoMcpOptions serverOptions)
    {
        services.Replace(ServiceDescriptor.Scoped<ITokenStore, TokenStore>());
        services.Replace(ServiceDescriptor.Scoped<IAccessTokenProvider, FargoAccessTokenProvider>());
        services.Replace(ServiceDescriptor.Scoped<IAuthenticationProvider>(sp =>
            new BaseBearerTokenAuthenticationProvider(sp.GetRequiredService<IAccessTokenProvider>())));

        // The base options were registered as a singleton for stdio mode; in HTTP
        // mode they are rebuilt per scope with the caller's token.
        services.RemoveAll<FargoMcpOptions>();
        services.AddScoped(sp =>
        {
            var header = sp.GetRequiredService<IHttpContextAccessor>()
                .HttpContext?.Request.Headers.Authorization.ToString();

            return new FargoMcpOptions
            {
                Server = serverOptions.Server,
                AccessToken = BearerToken.Parse(header)
            };
        });

        services.AddScoped<FargoAuthenticator>();

        return services;
    }
}
