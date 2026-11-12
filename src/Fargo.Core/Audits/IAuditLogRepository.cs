namespace Fargo.Core.Audits;

/// <summary>
/// Defines the repository contract for managing <see cref="AuditLog"/> entities.
/// /// </summary>
public interface IAuditLogRepository
{
    /// <summary>
    /// Adds an audit log to the repository.
    /// </summary>
    /// <param name="auditLog">
    /// The audit log to add.
    /// /// </param>
    void Add(AuditLog auditLog);
}
