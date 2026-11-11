using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Fargo.Infrastructure.DependencyInjection;

/// <summary>
/// Registers the integration-event publisher.
/// </summary>
public static class MessagingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers <see cref="RabbitMqEventPublisher"/> as the
        /// <see cref="IEventPublisher"/>. Requires an <see cref="IConnection"/>
        /// registration, for example from the Aspire RabbitMQ client integration
        /// (added by the host that publishes).
        /// </summary>
        internal IServiceCollection AddFargoMessaging(IConfiguration configuration)
        {
            services
            .AddOptions<RabbitMqEventOptions>()
            .Bind(configuration.GetSection(RabbitMqEventOptions.SectionName))
            .ValidateDataAnnotations();

            // The connection is resolved lazily, on the first publish, so the
            // host starts even while the broker is down.
            services.AddSingleton<IEventPublisher>(sp => new RabbitMqEventPublisher(
                () => sp.GetRequiredService<IConnection>(),
                sp.GetRequiredService<IOptions<RabbitMqEventOptions>>()));

            return services;
        }
    }
}
