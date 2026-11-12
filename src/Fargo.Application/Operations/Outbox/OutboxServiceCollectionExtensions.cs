using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Outbox.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Outbox;

/// <summary>
/// Registers the Outbox use-case handlers.
/// </summary>
public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the outbox publish and purge command handlers. They own
        /// their own transaction and claim locks, so they are registered
        /// directly, bypassing the unit-of-work, transaction and audit
        /// decorators of <c>AddFargoCommandHandler</c>.
        /// </summary>
        public IServiceCollection AddFargoOutboxApplication() => services
        .AddScoped<ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto>, PublishOutboxBatchCommandHandler>()
        .AddScoped<ICommandHandler<PurgeProcessedOutboxCommand, int>, PurgeProcessedOutboxCommandHandler>();
    }
}
