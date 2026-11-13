using Fargo.Client.Authentication;
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
    /// Replaces the singleton token plumbing of <c>AddFargoHttpClient</c> with scoped equivalents and builds
    /// each scope's <see cref="FargoMcpOptions"/> from the current request's bearer token, so one caller's token
    /// is never visible to another. Call after <c>AddFargoHttpClient</c> and <c>AddHttpContextAccessor</c>.
    /// </summary>
    public static IServiceCollection AddScopedFargoAuthentication(
        this IServiceCollection services,
        FargoMcpOptions serverOptions)
    {
        services.Replace(ServiceDescriptor.Scoped<ITokenStore, TokenStore>());
        // Everything that reads the token store must be scoped with it, or a
        // singleton would capture one caller's store.
        services.Replace(ServiceDescriptor.Scoped(sp => new FargoTokenRefresher(
            () => sp.GetRequiredService<IHttpClientFactory>().CreateClient(FargoAuthenticationServiceCollectionExtensions.HttpClientName),
            sp.GetRequiredService<ITokenStore>())));
        services.Replace(ServiceDescriptor.Scoped<IFargoAccessTokenSource, StoredAccessTokenSource>());
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
