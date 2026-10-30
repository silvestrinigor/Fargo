using Fargo.ClientHttp;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for reading Fargo service accounts. Read-only (list/get) by
/// design: creating, rotating secrets for, or deleting service accounts mints
/// or destroys machine credentials, which is a security-sensitive operation that
/// is deliberately NOT exposed as an agent-triggerable tool. Those actions are
/// available through the CLI / HTTP API under human control.
/// </summary>
[McpServerToolType]
public static class ServiceAccountTools
{
    [McpServerTool(Name = "fargo_serviceaccounts_list")]
    [Description("List service accounts (machine identities), optionally paginated. Secrets are never returned.")]
    public static async Task<IReadOnlyList<ServiceAccountSummary>> ListServiceAccountsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of service accounts to return per page.")] int? limit = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.V1.ServiceAccounts.GetAsync(request =>
        {
            if (page.HasValue)
            {
                request.QueryParameters.Page = page.Value;
            }

            if (limit.HasValue)
            {
                request.QueryParameters.Limit = limit.Value;
            }
        }, cancellationToken);

        return result?.Items?.Select(a => a.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_serviceaccounts_get")]
    [Description("Get a single service account by its GUID. The secret is never returned.")]
    public static async Task<ServiceAccountSummary?> GetServiceAccountAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account.")] Guid serviceAccountGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var account = await client.V1.ServiceAccounts[serviceAccountGuid].GetAsync(cancellationToken: cancellationToken);

        return account?.ToSummary();
    }
}
