using Fargo.Application.IntegrationEvents;
using Fargo.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Fargo.Infrastructure.DependencyInjection;

public static class MessagingServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Requires an <see cref="IConnection"/> registration, for example from the Aspire RabbitMQ client
        /// integration (added by the host that publishes).
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

        /// <summary>
        /// Registers <see cref="RabbitMqArticleEventConsumer"/> as a hosted service that subscribes to every
        /// article event. Opt-in, because it starts a background subscription: only the host that reacts to
        /// events (the real-time service) calls it.
        /// </summary>
        /// <remarks>
        /// Requires an <see cref="IArticleEventHandler"/> registration (provided by the host) and an
        /// <see cref="IConnection"/> registration.
        /// </remarks>
        public IServiceCollection AddFargoArticleEventConsumer(IConfiguration configuration)
        {
            services
            .AddOptions<RabbitMqConsumerOptions>()
            .Bind(configuration.GetSection(RabbitMqConsumerOptions.SectionName))
            .ValidateDataAnnotations();

            services.TryAddSingleton(TimeProvider.System);

            // The connection is resolved on each subscribe attempt, so the host
            // starts even while the broker is down.
            services.AddHostedService(sp => new RabbitMqArticleEventConsumer(
                () => sp.GetRequiredService<IConnection>(),
                sp.GetRequiredService<IArticleEventHandler>(),
                sp.GetRequiredService<IOptions<RabbitMqEventOptions>>(),
                sp.GetRequiredService<IOptions<RabbitMqConsumerOptions>>(),
                sp.GetRequiredService<TimeProvider>(),
                sp.GetRequiredService<ILogger<RabbitMqArticleEventConsumer>>()));

            return services;
        }
    }
}
