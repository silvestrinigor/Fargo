using Fargo.Core.Audits;
using Fargo.Infrastructure.Persistence;

namespace Fargo.Infrastructure.Repositories;

public sealed class AuditLogRepository(FargoDbContext context) : IAuditLogRepository
{
    public void Add(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }
}
