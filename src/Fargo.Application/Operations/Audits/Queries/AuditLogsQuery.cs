using Fargo.Application.Abstractions;
using Fargo.Application.Common.Actors;
using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.Audits.Dtos;
using Fargo.Application.Persistence.Repositories;
using Fargo.Core.Common;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits;

public sealed record AuditLogsQuery(
    Pagination WithPagination,
    Guid? ActorGuid = null,
    ActorType? ActorType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<PagedResult<AuditLogDto>>;

public sealed class AuditLogsQueryHandler(
    ActorResolver actorResolver,
    IAuditLogQueryRepository auditLogQueryRepository,
    ICurrentActor currentActor
) : IQueryHandler<AuditLogsQuery, PagedResult<AuditLogDto>>
{
    public async Task<PagedResult<AuditLogDto>> HandleAsync(AuditLogsQuery query, CancellationToken cancellationToken = default)
    {
        var actor = await actorResolver.ResolveCurrentActorOrThrowAsync(currentActor, cancellationToken);

        var accessScope = actor.ToAuditLogAccessScope();

        var result = await auditLogQueryRepository.GetManyInfoOrderedByOccurredAtAsync(
            query.WithPagination,
            accessScope,
            query.ActorGuid,
            query.ActorType,
            query.PeriodStart,
            query.PeriodEnd,
            cancellationToken
        );

        return result;
    }
}

internal static class AuditLogAccessScopeExtensions
{
    public static AuditLogAccessScope ToAuditLogAccessScope(this SecurityPrincipal actor)
        => new(
            actor.Guid,
            [.. actor.PartitionAccessGuids],
            actor.HasPartitionAccess(FargoCoreWellKnowGuids.GlobalPartitionGuid));
}
