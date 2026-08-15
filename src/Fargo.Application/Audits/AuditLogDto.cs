using Fargo.Core.Actors;
using Fargo.Core.Audits;
using Fargo.Core.Entities;

namespace Fargo.Application.Audits;

public sealed record AuditLogDto(
    Guid Guid,
    Guid ActorGuid,
    ActorType ActorType,
    ActionType ActionType,
    Guid EntityGuid,
    EntityType EntityType,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, AuditValue> Metadata,
    IReadOnlyCollection<Guid> Partitions
);
