using Fargo.ClientHttp.AuditLogs.Item;
using Microsoft.Kiota.Abstractions;

namespace Fargo.ClientHttp.AuditLogs;

/// <summary>
/// Hand-authored extension to the generated audit logs request builder adding
/// access to a single audit log by GUID (<c>GET /audit-logs/{auditLogGuid}</c>).
/// Kept in a partial class so it survives client regeneration.
/// </summary>
public partial class AuditLogsRequestBuilder
{
    /// <summary>Gets a single audit log item by its unique identifier.</summary>
    /// <param name="position">The unique identifier of the audit log.</param>
    public WithAuditLogGuidItemRequestBuilder this[Guid position]
    {
        get
        {
            var urlTplParams = new Dictionary<string, object>(PathParameters)
            {
                { "auditLogGuid", position },
            };

            return new WithAuditLogGuidItemRequestBuilder(urlTplParams, RequestAdapter);
        }
    }
}
