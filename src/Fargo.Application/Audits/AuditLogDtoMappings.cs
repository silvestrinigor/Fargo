using Fargo.Application.Shared.Audits;
using Fargo.Core.Audits;
using System.Linq.Expressions;

namespace Fargo.Application.Audits;

public static class AuditLogDtoMappings
{
    public static readonly Expression<Func<AuditLog, AuditLogDto>> Projection
        = auditLog => new AuditLogDto(
            auditLog.Guid);
}
