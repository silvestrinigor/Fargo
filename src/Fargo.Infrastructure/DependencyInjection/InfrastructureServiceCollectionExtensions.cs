using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Single entry point for the Fargo infrastructure layer, used by every
        /// host (http, migration, seed, outbox). Everything is registered
        /// unconditionally: DI registration is lazy, so a host only pays for the
        /// services it actually resolves. Host-specific pieces (JWT options,
        /// HTTP actor/client context, RabbitMQ publisher) are validated or
        /// connected only on first resolution, never at startup.
        /// </summary>
        /// <remarks>
        /// Host-only concerns stay in the host: the Aspire <c>AddRabbitMQClient</c>
        /// call (required for the event publisher to publish), the ASP.NET
        /// authentication pipeline, and the seed/outbox option sections.
        /// </remarks>
        public IServiceCollection AddFargoInfrastructure(IConfiguration configuration) => services
        .AddFargoPersistence(configuration)
        .AddFargoActorAuthorization(configuration)
        .AddFargoSecurityInfrastructure(configuration)
        .AddFargoMessaging(configuration);
    }
}
