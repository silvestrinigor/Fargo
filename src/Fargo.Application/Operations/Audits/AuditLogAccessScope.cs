namespace Fargo.Application.Operations.Audits;

/// <summary>
/// Which actors' audit logs a reader may see: its own, those of actors in a partition it can
/// access, or all of them with global access.
/// </summary>
public sealed record AuditLogAccessScope(
    Guid SelfActorGuid,
    IReadOnlyCollection<Guid> AccessiblePartitionGuids,
    bool HasGlobalAccess
);
