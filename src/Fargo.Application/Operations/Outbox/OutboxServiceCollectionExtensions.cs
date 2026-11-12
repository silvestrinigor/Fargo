using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Outbox.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Outbox;

public static class OutboxServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registered through the standard command pipeline: the publish command is transactional, so its
        /// decorator owns the transaction that holds the claim locks; both commands are exempt from audit.
        /// </summary>
        public IServiceCollection AddFargoOutboxApplication() => services
        .AddFargoCommandHandler<PublishOutboxBatchCommand, OutboxBatchResultDto, PublishOutboxBatchCommandHandler>()
        .AddFargoCommandHandler<PurgeProcessedOutboxCommand, int, PurgeProcessedOutboxCommandHandler>();
    }
}
