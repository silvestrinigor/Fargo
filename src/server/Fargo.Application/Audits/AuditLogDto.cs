using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Audits;

/// <summary>
/// Represents a data transfer object for a command audit log entry.
/// </summary>
/// <param name="Guid">The unique identifier of the audit log entry.</param>
/// <param name="ActorGuid">The unique identifier of the actor that executed the command.</param>
/// <param name="ActorType">The type of actor that executed the command.</param>
/// <param name="AuditLogType">The kind of command that was executed.</param>
/// <param name="CommandName">The type name of the executed command.</param>
/// <param name="OccurredAt">The date and time when the command was executed.</param>
/// <param name="Metadata">A snapshot of the executed command's data.</param>
public sealed record AuditLogDto(
    Guid Guid,
    Guid ActorGuid,
    ActorType ActorType,
    AuditLogType AuditLogType,
    string CommandName,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, AuditValue> Metadata
);
