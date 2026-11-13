using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace Fargo.Client.Realtime;

/// <summary>
/// Registers the <see cref="FargoRealtimeClient"/>.
/// </summary>
public static class RealtimeClientServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers a singleton <see cref="FargoRealtimeClient"/> for the hub at
        /// <paramref name="realtimeBaseAddress"/>.
        /// </summary>
        /// <remarks>
        /// The client takes its tokens from the registered <see cref="IFargoRealtimeAccessTokenProvider"/>,
        /// which the application must register.
        /// </remarks>
        public IServiceCollection AddFargoRealtimeClient(
            Uri realtimeBaseAddress,
            Action<FargoRealtimeClientOptions>? configure = null)
        {
            ArgumentNullException.ThrowIfNull(realtimeBaseAddress);

            var options = new FargoRealtimeClientOptions { RealtimeBaseAddress = realtimeBaseAddress };
            configure?.Invoke(options);

            services.TryAddSingleton(sp => new FargoRealtimeClient(
                options,
                sp.GetRequiredService<IFargoRealtimeAccessTokenProvider>(),
                sp.GetService<ILogger<FargoRealtimeClient>>()));

            return services;
        }
    }
}
