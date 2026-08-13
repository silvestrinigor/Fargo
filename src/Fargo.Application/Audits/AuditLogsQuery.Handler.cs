using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

public sealed class AuditLogsQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor,
    ILogger<AuditLogsQueryHandler> logger
) : IQueryHandler<AuditLogsQuery, IReadOnlyCollection<AuditLogDto>>
{
    public async Task<IReadOnlyCollection<AuditLogDto>> HandleAsync(AuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        logger.ManyQueryStarted(currentActor.Guid, query.WithPagination.Page, query.WithPagination.Limit);

        var actor = await actorResolver.GetActorByGuidAndTypeAsync(currentActor.Guid, currentActor.ActorType, cancellationToken);

        ActorNotFoundFargoApplicationException.ThrowIfNull(actor, currentActor.Guid, currentActor.ActorType);

        var items = await auditLogQueryRepository.GetManyInfoAsync(
            query.WithPagination,
            actor.PartitionAccessGuids,
            cancellationToken);

        logger.ManyQueryCompleted(currentActor.Guid, actor.PartitionAccessGuids.Count, items.Count);

        return items;
    }
}
