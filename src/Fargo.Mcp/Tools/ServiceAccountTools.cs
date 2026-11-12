using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Creating an account and rotating its secret return the new client secret ONCE in the tool result (the
/// server never stores or re-reveals it). That secret therefore passes through the model's context and the
/// MCP client's logs, so these tools should only be approved for trusted sessions. List and get never return
/// secrets.
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

    [McpServerTool(Name = "fargo_serviceaccounts_create")]
    [Description("Create a new service account. The generated client secret is returned EXACTLY ONCE in this result and cannot be retrieved later (only rotated), so hand it to the user to store securely.")]
    public static async Task<ServiceAccountCreatedSummary> CreateServiceAccountAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Unique client identifier (nameid format) of the service account.")] string clientId,
        [Description("Optional human-readable description.")] string? description = null,
        [Description("Whether the account is active. Defaults to true.")] bool isActive = true,
        [Description("Permissions to grant; requires the ChangeServiceAccountPermissions permission. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("GUIDs of the partitions the account belongs to (its membership / scope).")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of the partitions the account is granted access to.")] Guid[]? partitionAccessesToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ServiceAccountCreateDto
        {
            ClientId = clientId,
            Description = description,
            IsActive = isActive,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd)
        };

        var created = await client.V1.ServiceAccounts.PostAsync(body, cancellationToken: cancellationToken);

        return created?.ToCreatedSummary()
            ?? throw new InvalidOperationException("The server returned no content for the created service account.");
    }

    [McpServerTool(Name = "fargo_serviceaccounts_update")]
    [Description("Update an existing service account. Only the supplied fields are changed; omitted fields are left as-is. Partition membership and partition access are managed independently.")]
    public static async Task<string> UpdateServiceAccountAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account to update.")] Guid serviceAccountGuid,
        [Description("The new description.")] string? description = null,
        [Description("GUIDs of partitions to add to the account's membership.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of partitions to remove from the account's membership.")] Guid[]? partitionsToRemove = null,
        [Description("GUIDs of partitions to grant the account access to.")] Guid[]? partitionAccessesToAdd = null,
        [Description("GUIDs of partitions to revoke the account's access from.")] Guid[]? partitionAccessesToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ServiceAccountUpdateDto
        {
            Description = description,
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionsToRemove = ToolInput.ToGuidList(partitionsToRemove),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd),
            PartitionAccessesToRemove = ToolInput.ToGuidList(partitionAccessesToRemove)
        };

        await client.V1.ServiceAccounts[serviceAccountGuid].PutAsync(body, cancellationToken: cancellationToken);

        return $"Service account '{serviceAccountGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_serviceaccounts_set_permissions")]
    [Description("Grant and revoke permissions of a service account in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. The caller cannot change its own permissions. Requires the ChangeServiceAccountPermissions permission, which is separate from the permission to update.")]
    public static async Task<string> SetServiceAccountPermissionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account.")] Guid serviceAccountGuid,
        [Description("Permissions to grant. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("Permissions to revoke (same codes as permissionsToAdd).")] int[]? permissionsToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new PermissionsChangeDto
        {
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PermissionsToRemove = ToolInput.ToIntList(permissionsToRemove)
        };

        await client.V1.ServiceAccounts[serviceAccountGuid].Permissions.PutAsync(body, cancellationToken: cancellationToken);

        return $"Service account '{serviceAccountGuid}' permissions updated.";
    }

    [McpServerTool(Name = "fargo_serviceaccounts_set_active")]
    [Description("Activate or deactivate a service account. Requires the ActivateServiceAccount permission, which is separate from the permission to update.")]
    public static async Task<string> SetServiceAccountActiveAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account.")] Guid serviceAccountGuid,
        [Description("true to activate, false to deactivate.")] bool isActive,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.ServiceAccounts[serviceAccountGuid].IsActive.PutAsync(
            new ActiveStateDto { IsActive = isActive },
            cancellationToken: cancellationToken);

        return $"Service account '{serviceAccountGuid}' {(isActive ? "activated" : "deactivated")}.";
    }

    [McpServerTool(Name = "fargo_serviceaccounts_rotate_secret")]
    [Description("Rotate a service account's secret. The old secret stops working immediately. The new secret is returned EXACTLY ONCE in this result and cannot be retrieved later, so hand it to the user to store securely.")]
    public static async Task<ServiceAccountCreatedSummary> RotateServiceAccountSecretAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account whose secret to rotate.")] Guid serviceAccountGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var rotated = await client.V1.ServiceAccounts[serviceAccountGuid].RotateSecret
            .PostAsync(cancellationToken: cancellationToken);

        return rotated?.ToCreatedSummary()
            ?? throw new InvalidOperationException("The server returned no content for the rotated secret.");
    }

    [McpServerTool(Name = "fargo_serviceaccounts_delete")]
    [Description("Delete a service account by its GUID. Anything authenticating with it stops working. This cannot be undone.")]
    public static async Task<string> DeleteServiceAccountAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the service account to delete.")] Guid serviceAccountGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.ServiceAccounts[serviceAccountGuid].DeleteAsync(cancellationToken: cancellationToken);

        return $"Service account '{serviceAccountGuid}' deleted.";
    }
}
