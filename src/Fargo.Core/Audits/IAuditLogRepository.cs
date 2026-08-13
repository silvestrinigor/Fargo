namespace Fargo.Core.Audits;

public interface IAuditLogRepository
{
    void Add(AuditLog auditLog);
}
