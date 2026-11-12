using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits.Dtos;

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

/// <summary>
/// Provides mapping extensions for converting AuditLog entities to AuditLogDto data transfer objects.
/// This class contains the logic for transforming audit log domain objects into DTOs suitable for API responses.
/// </summary>
public static class AuditLogDtoMappings
{
    /// <summary>
    /// Converts an AuditLog entity to an AuditLogDto data transfer object.
    /// </summary>
    /// <param name="auditLog">The AuditLog entity to convert</param>
    /// <returns>A new AuditLogDto instance populated with data from the audit log entity</returns>
    public static AuditLogDto ToDto(this AuditLog auditLog)
    {
        return new AuditLogDto(
            auditLog.Guid,
            auditLog.ActorGuid,
            auditLog.ActorType,
            auditLog.AuditLogType,
            auditLog.CommandName,
            auditLog.OccurredAt,
            auditLog.Metadata.Values
        );
    }
}
