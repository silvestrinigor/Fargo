using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

public interface IAuditLogQueryRepository
{
    Task<IReadOnlyCollection<AuditLogDto>> GetManyInfoOrderedByOccurredAtAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Guid? actorGuid = null,
        ActorType? actorType = null,
        Guid? entityGuid = null,
        EntityType? entityType = null,
        DateTimeOffset? periodStart = null,
        DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default
    );
}
