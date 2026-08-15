using Fargo.Core.Audits;

namespace Fargo.Application.Audits;

public static class AuditLogDtoMappings
{
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
