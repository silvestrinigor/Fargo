namespace Fargo.Application.Audits;

/// <summary>
/// Describes which actors' command audit logs a reader may access.
/// </summary>
/// <remarks>
/// Audit logs are actor-scoped: a reader may see the audits of actors it has
/// access to. An actor is accessible when the reader has access to any partition
/// that actor belongs to, and a reader can always see its own audits. A reader
/// with global-partition access sees every actor's audits.
/// </remarks>
/// <param name="SelfActorGuid">
/// The reader's own actor identifier; the reader can always read its own audits.
/// </param>
/// <param name="AccessiblePartitionGuids">
/// The partitions the reader can access. An actor is accessible when it belongs
/// to any of these partitions.
/// </param>
/// <param name="HasGlobalAccess">
/// When <see langword="true"/>, the reader can access every actor's audits and
/// the partition/self filtering is bypassed.
/// </param>
public sealed record AuditLogAccessScope(
    Guid SelfActorGuid,
    IReadOnlyCollection<Guid> AccessiblePartitionGuids,
    bool HasGlobalAccess
);
