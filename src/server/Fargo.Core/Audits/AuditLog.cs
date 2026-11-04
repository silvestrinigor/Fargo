using Fargo.Core.Entities.Actors;

namespace Fargo.Core.Audits;

/// <summary>
/// Represents an immutable record of a command an actor executed.
/// </summary>
/// <remarks>
/// An audit log describes the intended command an actor ran: which actor
/// (<see cref="ActorGuid"/>/<see cref="ActorType"/>), the kind of command
/// (<see cref="AuditLogType"/>), the command's type name
/// (<see cref="CommandName"/>), and a snapshot of the command's data
/// (<see cref="Metadata"/>).
///
/// Audit logs relate directly to the acting actor and are removed when that
/// actor is deleted. Read access is actor-scoped: only an actor with access to
/// the acting actor can read that actor's command audit logs. Audit logs are not
/// associated with partitions.
/// </remarks>
public class AuditLog
{
    /// <summary>
    /// Gets the unique identifier of the audit log.
    /// </summary>
    public Guid Guid { get; private set; } = Guid.NewGuid();

    /// <summary>
    /// Gets the identifier of the actor that executed the command.
    /// </summary>
    public Guid ActorGuid { get; private init; }

    /// <summary>
    /// Gets the type of actor that executed the command.
    /// </summary>
    public ActorType ActorType { get; private init; }

    /// <summary>
    /// Gets the kind of command recorded by this audit log.
    /// </summary>
    public AuditLogType AuditLogType { get; private init; }

    /// <summary>
    /// Gets the type name of the command that was executed.
    /// </summary>
    public string CommandName { get; private init; } = null!;

    /// <summary>
    /// Gets the date and time at which the command was executed.
    /// </summary>
    public DateTimeOffset OccurredAt { get; private init; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Gets a snapshot of the executed command's data.
    /// </summary>
    public AuditMetadata Metadata { get; private init; } = new();

    private AuditLog() { }

    private AuditLog(Guid actorGuid, ActorType actorType, AuditLogType auditLogType, string commandName, AuditMetadata metadata)
    {
        ActorGuid = actorGuid;
        ActorType = actorType;
        AuditLogType = auditLogType;
        CommandName = commandName;
        Metadata = metadata;
    }

    /// <summary>
    /// Creates an audit log describing a command executed by an actor.
    /// </summary>
    /// <param name="actorGuid">The identifier of the actor that executed the command.</param>
    /// <param name="actorType">The type of actor that executed the command.</param>
    /// <param name="auditLogType">The kind of command executed.</param>
    /// <param name="commandName">The type name of the executed command.</param>
    /// <param name="metadata">A snapshot of the command's data.</param>
    /// <returns>A new <see cref="AuditLog"/> instance.</returns>
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
