using Fargo.Application.Common;
using Fargo.Core.Actors;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

public sealed record AuditLogsQuery(
    Pagination WithPagination,
    Guid? ActorGuid = null,
    ActorType? ActorType = null,
    Guid? EntityGuid = null,
    EntityType? EntityType = null,
    DateTimeOffset? PeriodStart = null,
    DateTimeOffset? PeriodEnd = null
) : IQuery<IReadOnlyCollection<AuditLogDto>>;
