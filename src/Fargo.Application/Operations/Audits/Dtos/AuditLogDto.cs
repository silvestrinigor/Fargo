using Fargo.Core.Audits;
using Fargo.Core.Entities.Actors;

namespace Fargo.Application.Operations.Audits.Dtos;

public sealed record AuditLogDto(
    Guid Guid,
    Guid ActorGuid,
    ActorType ActorType,
    AuditLogType AuditLogType,
    string CommandName,
    DateTimeOffset OccurredAt,
    IReadOnlyDictionary<string, AuditValue> Metadata
);

public static class AuditLogDtoMappings
{
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
