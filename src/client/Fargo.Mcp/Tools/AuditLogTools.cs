using System.ComponentModel;
using Fargo.ClientHttp;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for reading Fargo audit logs. Audit logs are read-only.
/// </summary>
[McpServerToolType]
public static class AuditLogTools
{
    [McpServerTool(Name = "fargo_auditlogs_list")]
    [Description("List audit log entries, optionally filtered by actor, affected entity, and time period.")]
    public static async Task<IReadOnlyList<AuditLogSummary>> ListAuditLogsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of audit log entries to return per page.")] int? limit = null,
        [Description("Only return entries for the actor with this GUID.")] Guid? actorGuid = null,
        [Description("Only return entries for this numeric actor type.")] int? actorType = null,
        [Description("Only return entries affecting the entity with this GUID.")] Guid? entityGuid = null,
        [Description("Only return entries affecting this numeric entity type.")] int? entityType = null,
        [Description("Only return entries that occurred at or after this UTC timestamp (ISO 8601).")] DateTimeOffset? periodStart = null,
        [Description("Only return entries that occurred at or before this UTC timestamp (ISO 8601).")] DateTimeOffset? periodEnd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.AuditLogs.GetAsync(request =>
        {
            if (page.HasValue)
            {
                request.QueryParameters.Page = page.Value;
            }

            if (limit.HasValue)
            {
                request.QueryParameters.Limit = limit.Value;
            }

            if (actorGuid.HasValue)
            {
                request.QueryParameters.ActorGuid = actorGuid.Value;
            }

            if (actorType.HasValue)
            {
                request.QueryParameters.ActorType = actorType.Value;
            }

            if (entityGuid.HasValue)
            {
                request.QueryParameters.EntityGuid = entityGuid.Value;
            }

            if (entityType.HasValue)
            {
                request.QueryParameters.EntityType = entityType.Value;
            }

            if (periodStart.HasValue)
            {
                request.QueryParameters.PeriodStart = periodStart.Value;
            }

            if (periodEnd.HasValue)
            {
                request.QueryParameters.PeriodEnd = periodEnd.Value;
            }
        }, cancellationToken);

        return result?.Select(a => a.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_auditlogs_get")]
    [Description("Get a single audit log entry by its GUID.")]
    public static async Task<AuditLogSummary?> GetAuditLogAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the audit log entry.")] Guid auditLogGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var entry = await client.AuditLogs[auditLogGuid].GetAsync(cancellationToken: cancellationToken);

        return entry?.ToSummary();
    }
}
