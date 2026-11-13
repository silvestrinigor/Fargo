using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Fargo.Client.Authentication;

/// <summary>
/// Registers the token storage and refresh used by the Fargo HTTP client.
/// </summary>
public static class FargoAuthenticationServiceCollectionExtensions
{
    /// <summary>
    /// The name of the <see cref="HttpClient"/> used to refresh tokens.
    /// </summary>
    public const string HttpClientName = "Fargo.Client.TokenRefresh";

    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the token store, the <see cref="FargoTokenRefresher"/> and
        /// the default <see cref="IFargoAccessTokenSource"/>
        /// (<see cref="StoredAccessTokenSource"/>, which never refreshes).
        /// </summary>
        /// <remarks>
        /// Existing registrations of these services are kept, so an application can register its own store.
        /// </remarks>
        public IServiceCollection AddFargoClientAuthentication(Uri apiBaseAddress)
        {
            ArgumentNullException.ThrowIfNull(apiBaseAddress);

            return services.AddFargoClientAuthentication(_ => apiBaseAddress);
        }

        /// <summary>
        /// Registers the client authentication services, reading the API
        /// base address when the refresh client is created, so it can follow a
        /// runtime override (for example a <c>--server</c> option).
        /// </summary>
        public IServiceCollection AddFargoClientAuthentication(Func<IServiceProvider, Uri> apiBaseAddress)
        {
            ArgumentNullException.ThrowIfNull(apiBaseAddress);

            services.AddHttpClient(HttpClientName, (sp, client) => client.BaseAddress = apiBaseAddress(sp));

            services.TryAddSingleton<ITokenStore, TokenStore>();

            // One refresher per token store: its lock is what keeps two callers
            // from sending the same single-use refresh token.
            services.TryAddSingleton(sp => new FargoTokenRefresher(
                () => sp.GetRequiredService<IHttpClientFactory>().CreateClient(HttpClientName),
                sp.GetRequiredService<ITokenStore>()));

            services.TryAddSingleton<IFargoAccessTokenSource, StoredAccessTokenSource>();

            return services;
        }

        /// <summary>
        /// Makes clients refresh the stored tokens shortly before they expire,
        /// by replacing the <see cref="IFargoAccessTokenSource"/> with
        /// <see cref="RefreshingAccessTokenSource"/>. Call after
        /// <c>AddFargoClientAuthentication</c>.
        /// </summary>
        public IServiceCollection UseFargoRefreshingAccessTokens()
        {
            services.Replace(ServiceDescriptor.Singleton<IFargoAccessTokenSource, RefreshingAccessTokenSource>());

            return services;
        }
    }
}
