using Fargo.ClientAuthentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Fargo.ClientRealtime;

/// <summary>
/// Registers the <see cref="FargoRealtimeClient"/>.
/// </summary>
public static class RealtimeClientServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers a singleton <see cref="FargoRealtimeClient"/> and the shared
        /// client authentication services for the API at
        /// <paramref name="apiBaseAddress"/>.
        /// </summary>
        /// <remarks>
        /// The client uses the registered <see cref="IFargoAccessTokenSource"/>,
        /// which by default returns the stored token without refreshing it. Call
        /// <c>UseFargoRefreshingAccessTokens()</c> to refresh before expiry, or
        /// register your own source.
        /// </remarks>
        /// <param name="apiBaseAddress">The base address of the Fargo API, used to refresh tokens.</param>
        /// <param name="realtimeBaseAddress">The base address of the real-time service.</param>
        /// <param name="configure">Optionally adjusts the client options.</param>
        public IServiceCollection AddFargoRealtimeClient(
            Uri apiBaseAddress,
            Uri realtimeBaseAddress,
            Action<FargoRealtimeClientOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(apiBaseAddress);
            ArgumentNullException.ThrowIfNull(realtimeBaseAddress);

            services.AddFargoClientAuthentication(apiBaseAddress);

            var options = new FargoRealtimeClientOptions { RealtimeBaseAddress = realtimeBaseAddress };
            configure?.Invoke(options);

            services.TryAddSingleton(sp => new FargoRealtimeClient(
                options,
                sp.GetRequiredService<IFargoAccessTokenSource>(),
                sp.GetService<ILogger<FargoRealtimeClient>>()));

            return services;
        }
    }
}
