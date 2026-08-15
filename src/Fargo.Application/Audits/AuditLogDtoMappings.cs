using Fargo.Core.Audits;
using System.Linq.Expressions;

namespace Fargo.Application.Audits;

public static class AuditLogDtoMappings
{
    public static readonly Expression<Func<AuditLog, AuditLogDto>> Projection = auditLog 
        => new AuditLogDto(
            auditLog.Guid,
            auditLog.ActorGuid,
            auditLog.ActorType,
            auditLog.ActionType,
            auditLog.EntityGuid,
            auditLog.EntityType,
            auditLog.OccurredAt,
            auditLog.Metadata.Values,
            auditLog.PartitionGuids
        );
}
