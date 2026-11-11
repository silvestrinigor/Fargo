using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for working with Fargo users. Mirrors the <c>fargo user</c> CLI
/// surface. Passwords are accepted as tool arguments (create and admin reset) and
/// are never returned.
/// </summary>
[McpServerToolType]
public static class UserTools
{
    [McpServerTool(Name = "fargo_users_list")]
    [Description("List users, optionally paginated, searched, sorted and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<UserSummary>> ListUsersAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of users to return per page.")] int? limit = null,
        [Description("Only return users that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        [Description("Case-insensitive text to match against the user nameid.")] string? search = null,
        [Description("Sort field, optionally prefixed with '-' for descending (e.g. nameid, -nameid). Fields: guid, nameid, firstname, lastname, isactive.")] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.V1.Users.GetAsync(request =>
        {
            if (page.HasValue)
            {
                request.QueryParameters.Page = page.Value;
            }

            if (limit.HasValue)
            {
                request.QueryParameters.Limit = limit.Value;
            }

            if (partitions is { Length: > 0 })
            {
                request.QueryParameters.ChildOfAnyOfThesePartitions =
                    partitions.Select(g => (Guid?)g).ToArray();
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                request.QueryParameters.Search = search;
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                request.QueryParameters.Sort = sort;
            }
        }, cancellationToken);

        return result?.Items?.Select(u => u.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_users_get")]
    [Description("Get a single user by its GUID.")]
    public static async Task<UserSummary?> GetUserAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var user = await client.V1.Users[userGuid].GetAsync(cancellationToken: cancellationToken);

        return user?.ToSummary();
    }

    [McpServerTool(Name = "fargo_users_create")]
    [Description("Create a new user. Without a password the user cannot sign in with credentials. Returns the GUID of the created user.")]
    public static async Task<Guid?> CreateUserAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Unique username (nameid) of the new user.")] string nameid,
        [Description("Initial password for the user.")] string? password = null,
        [Description("How long a password stays valid by default, as an ISO-8601 duration, e.g. 'P90D'. Omit for no expiration.")] string? defaultPasswordExpirationPeriod = null,
        [Description("First name of the user.")] string? firstName = null,
        [Description("Last name of the user.")] string? lastName = null,
        [Description("Description of the user.")] string? description = null,
        [Description("Whether the user is active. Defaults to the server default (active).")] bool? isActive = null,
        [Description("Permissions to grant the user directly. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("GUIDs of the partitions the user belongs to (its membership).")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of the user groups to add the user to.")] Guid[]? userGroupsToAdd = null,
        [Description("GUIDs of the partitions the user is granted access to.")] Guid[]? partitionAccessesToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserCreateDto
        {
            Nameid = nameid,
            FirstName = firstName,
            LastName = lastName,
            Description = description,
            IsActive = isActive,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            UserGroupsToAdd = ToolInput.ToGuidList(userGroupsToAdd),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd)
        };

        if (!string.IsNullOrEmpty(password) || !string.IsNullOrWhiteSpace(defaultPasswordExpirationPeriod))
        {
            body.Authentication = new UserAuthenticationCreateDto
            {
                Password = string.IsNullOrEmpty(password) ? null : password,
                DefaultPasswordExpirationPeriod = string.IsNullOrWhiteSpace(defaultPasswordExpirationPeriod)
                    ? null
                    : defaultPasswordExpirationPeriod
            };
        }

        return await client.V1.Users.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_users_update")]
    [Description("Update an existing user. Only the supplied fields are changed; omitted fields are left as-is. Supplying a password resets it (requires the permission to change another user's password).")]
    public static async Task<string> UpdateUserAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user to update.")] Guid userGuid,
        [Description("The new username (nameid).")] string? nameid = null,
        [Description("The new first name.")] string? firstName = null,
        [Description("The new last name.")] string? lastName = null,
        [Description("The new description.")] string? description = null,
        [Description("Set the active state.")] bool? isActive = null,
        [Description("A new password to set for the user.")] string? password = null,
        [Description("The new default password validity as an ISO-8601 duration, e.g. 'P90D'.")] string? defaultPasswordExpirationPeriod = null,
        [Description("Set true to clear the default password expiration period.")] bool? removeDefaultPasswordExpirationPeriod = null,
        [Description("Permissions to grant. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("Permissions to revoke (same codes as permissionsToAdd).")] int[]? permissionsToRemove = null,
        [Description("GUIDs of partitions to add to the user's membership.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of partitions to remove from the user's membership.")] Guid[]? partitionsToRemove = null,
        [Description("GUIDs of user groups to add the user to.")] Guid[]? userGroupsToAdd = null,
        [Description("GUIDs of user groups to remove the user from.")] Guid[]? userGroupsToRemove = null,
        [Description("GUIDs of partitions to grant the user access to.")] Guid[]? partitionAccessesToAdd = null,
        [Description("GUIDs of partitions to revoke the user's access from.")] Guid[]? partitionAccessesToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserUpdateDto
        {
            Nameid = nameid,
            FirstName = firstName,
            LastName = lastName,
            Description = description,
            IsActive = isActive,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PermissionsToRemove = ToolInput.ToIntList(permissionsToRemove),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionsToRemove = ToolInput.ToGuidList(partitionsToRemove),
            UserGroupsToAdd = ToolInput.ToGuidList(userGroupsToAdd),
            UserGroupsToRemove = ToolInput.ToGuidList(userGroupsToRemove),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd),
            PartitionAccessesToRemove = ToolInput.ToGuidList(partitionAccessesToRemove)
        };

        if (!string.IsNullOrEmpty(password)
            || !string.IsNullOrWhiteSpace(defaultPasswordExpirationPeriod)
            || removeDefaultPasswordExpirationPeriod is not null)
        {
            body.Authentication = new UserAuthenticationUpdateDto
            {
                Password = string.IsNullOrEmpty(password) ? null : password,
                DefaultPasswordExpirationPeriod = string.IsNullOrWhiteSpace(defaultPasswordExpirationPeriod)
                    ? null
                    : defaultPasswordExpirationPeriod,
                RemoveDefaultPasswordExpirationPeriod = removeDefaultPasswordExpirationPeriod
            };
        }

        await client.V1.Users[userGuid].PutAsync(body, cancellationToken: cancellationToken);

        return $"User '{userGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_users_delete")]
    [Description("Delete a user by its GUID. This cannot be undone.")]
    public static async Task<string> DeleteUserAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user to delete.")] Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Users[userGuid].DeleteAsync(cancellationToken: cancellationToken);

        return $"User '{userGuid}' deleted.";
    }
}
