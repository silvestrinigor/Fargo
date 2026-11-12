using Fargo.Core.Entities.Actors;

namespace Fargo.Core.Audits;

/// <summary>
/// An immutable record of a command an actor executed, with a snapshot of the command's data.
/// </summary>
/// <remarks>
/// Audit logs are removed when the acting actor is deleted. Read access is actor-scoped: only an
/// actor with access to the acting actor can read its audit logs. Audit logs are not associated
/// with partitions.
/// </remarks>
public class AuditLog
{
    public Guid Guid { get; private set; } = Guid.NewGuid();

    public Guid ActorGuid { get; private init; }

    public ActorType ActorType { get; private init; }

    public AuditLogType AuditLogType { get; private init; }

    public string CommandName { get; private init; } = null!;

    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

    public AuditMetadata Metadata { get; private init; } = new();

    // Required by EF Core.
    private AuditLog() { }

    private AuditLog(Guid actorGuid, ActorType actorType, AuditLogType auditLogType, string commandName, AuditMetadata metadata)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        AuditLogType = auditLogType;
        CommandName = commandName;
        Metadata = metadata;
    }

    public static AuditLog ForCommand(
        Guid actorGuid,
        ActorType actorType,
        AuditLogType auditLogType,
        string commandName,
        AuditMetadata metadata)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(commandName);
        ArgumentNullException.ThrowIfNull(metadata);

        return new AuditLog(actorGuid, actorType, auditLogType, commandName, metadata);
    }
}
