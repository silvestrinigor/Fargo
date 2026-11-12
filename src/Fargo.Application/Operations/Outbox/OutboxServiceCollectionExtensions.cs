using Fargo.Application.Abstractions;
using Fargo.Application.Operations.Outbox.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Outbox;

public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// The handlers own their transaction and claim locks, so they are registered directly, without the
        /// unit-of-work, transaction and audit decorators of <c>AddFargoCommandHandler</c>.
        /// </summary>
        public IServiceCollection AddFargoOutboxApplication() => services
        .AddScoped<ICommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto>, PublishOutboxBatchCommandHandler>()
        .AddScoped<ICommandHandler<PurgeProcessedOutboxCommand, int>, PurgeProcessedOutboxCommandHandler>();
    }
}
