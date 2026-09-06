using Fargo.Core.Audits;

namespace Fargo.Application.Audits;

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
            auditLog.ActionType,
            auditLog.EntityGuid,
            auditLog.EntityType,
            auditLog.OccurredAt,
            auditLog.Metadata.Values,
            [.. auditLog.Partitions.Select(p => p.PartitionGuid)]
        );
    }
}
