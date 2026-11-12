using Fargo.Core.Entities.Actors;
using Fargo.Infrastructure.Authorization;
using Fargo.Infrastructure.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

public static class ActorAuthorizationServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the <see cref="IActorAuthorizationProvider"/> and the
        /// <see cref="IActorAuthorizationCache"/> chosen by
        /// <see cref="ActorAuthorizationCacheOptions.Provider"/>. To move to another backend, register a
        /// different <see cref="IActorAuthorizationCache"/>; to share entries across instances with the
        /// default provider, register an <c>IDistributedCache</c> (for example Redis).
        /// </summary>
        internal IServiceCollection AddFargoActorAuthorization(IConfiguration configuration)
        {
            services.AddScoped<ActorAuthorizationStateReader>();
            services.AddScoped<IActorAuthorizationProvider, ActorAuthorizationProvider>();

            var section = configuration.GetSection(ActorAuthorizationCacheOptions.SectionName);

            services
            .AddOptions<ActorAuthorizationCacheOptions>()
            .Bind(section)
            .ValidateDataAnnotations();

            var provider = section.Get<ActorAuthorizationCacheOptions>()?.Provider
                ?? ActorAuthorizationCacheProvider.Hybrid;

            if (provider == ActorAuthorizationCacheProvider.None)
            {
                services.AddSingleton<IActorAuthorizationCache, NoActorAuthorizationCache>();

                return services;
            }

            services.AddHybridCache();
            services.AddSingleton<IActorAuthorizationCache, HybridActorAuthorizationCache>();

            return services;
        }
    }
}
