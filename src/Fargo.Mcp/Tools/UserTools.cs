using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Passwords are accepted as tool arguments (create and admin reset) and are never returned.
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
                    [.. partitions.Select(g => (Guid?)g)];
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
        [Description("Permissions to grant the user directly; requires the ChangeUserPermissions permission. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("GUIDs of the partitions the user belongs to (its membership). Requires the ChangeUserPartitions permission.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of the user groups to add the user to. Requires the ChangeUserGroupMembers permission, and the caller must hold the permissions of those groups and their ancestors.")] Guid[]? userGroupsToAdd = null,
        [Description("GUIDs of the partitions the user is granted access to. Requires the ChangeUserPartitions permission.")] Guid[]? partitionAccessesToAdd = null,
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
    [Description("Update the username, names or description of an existing user. Only the supplied fields are changed; omitted fields are left as-is. Activation, permissions, user groups, partitions and passwords have their own tools.")]
    public static async Task<string> UpdateUserAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user to update.")] Guid userGuid,
        [Description("The new username (nameid).")] string? nameid = null,
        [Description("The new first name.")] string? firstName = null,
        [Description("The new last name.")] string? lastName = null,
        [Description("The new description.")] string? description = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserUpdateDto
        {
            Nameid = nameid,
            FirstName = firstName,
            LastName = lastName,
            Description = description
        };

        await client.V1.Users[userGuid].PutAsync(body, cancellationToken: cancellationToken);

        return $"User '{userGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_users_set_user_groups")]
    [Description("Add a user to, and remove it from, user groups in one request. At least one group is required and none may be in both lists. Joining a group hands the user the permissions of that group and of its ancestors, so the caller must hold all of them. Requires the ChangeUserGroupMembers permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserUserGroupsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        [Description("GUIDs of user groups to add the user to.")] Guid[]? userGroupsToAdd = null,
        [Description("GUIDs of user groups to remove the user from.")] Guid[]? userGroupsToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserGroupsChangeDto
        {
            UserGroupsToAdd = ToolInput.ToGuidList(userGroupsToAdd),
            UserGroupsToRemove = ToolInput.ToGuidList(userGroupsToRemove)
        };

        await client.V1.Users[userGuid].UserGroups.PutAsync(body, cancellationToken: cancellationToken);

        return $"User '{userGuid}' user groups updated.";
    }
    [McpServerTool(Name = "fargo_users_set_partitions")]
    [Description("Change the partitions of a user in one request: the partitions it belongs to (membership) and the partitions it can access. At least one list is required and no partition may be in both the add and remove list of the same kind. The caller needs access to every partition it touches. Requires the ChangeUserPartitions permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserPartitionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the a user.")] Guid userGuid,
        [Description("GUIDs of partitions to add to the membership.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of partitions to remove from the membership.")] Guid[]? partitionsToRemove = null,
        [Description("GUIDs of partitions to grant access to.")] Guid[]? partitionAccessesToAdd = null,
        [Description("GUIDs of partitions to revoke access from.")] Guid[]? partitionAccessesToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new PartitionsChangeDto
        {
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionsToRemove = ToolInput.ToGuidList(partitionsToRemove),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd),
            PartitionAccessesToRemove = ToolInput.ToGuidList(partitionAccessesToRemove)
        };

        await client.V1.Users[userGuid].Partitions.PutAsync(body, cancellationToken: cancellationToken);

        return $"User '{userGuid}' partitions updated.";
    }
    [McpServerTool(Name = "fargo_users_reset_password")]
    [Description("Reset a user's password. The user must choose a new password at the next sign-in. A caller cannot reset its own password this way. Requires the ChangeAnotherUserPassword permission.")]
    public static async Task<string> ResetUserPasswordAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        [Description("The new temporary password.")] string password,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Users[userGuid].Password.PutAsync(
            new PasswordResetDto { Password = password },
            cancellationToken: cancellationToken);

        return $"User '{userGuid}' password reset.";
    }

    [McpServerTool(Name = "fargo_users_set_password_expiration")]
    [Description("Set how long a user's passwords last by default, as an ISO-8601 duration such as 'P90D'. It applies the next time the password is changed; 'PT0S' means it must be changed immediately. Requires the ChangeAnotherUserPassword permission.")]
    public static async Task<string> SetUserPasswordExpirationAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        [Description("The expiration period as an ISO-8601 duration, e.g. 'P90D'.")] string expirationPeriod,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Users[userGuid].PasswordExpiration.PutAsync(
            new PasswordExpirationDto { ExpirationPeriod = expirationPeriod },
            cancellationToken: cancellationToken);

        return $"User '{userGuid}' password expiration set to {expirationPeriod}.";
    }

    [McpServerTool(Name = "fargo_users_remove_password_expiration")]
    [Description("Stop a user's passwords from expiring by default. Requires the ChangeAnotherUserPassword permission.")]
    public static async Task<string> RemoveUserPasswordExpirationAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Users[userGuid].PasswordExpiration.DeleteAsync(cancellationToken: cancellationToken);

        return $"User '{userGuid}' password expiration removed.";
    }
    [McpServerTool(Name = "fargo_users_set_permissions")]
    [Description("Grant and revoke permissions of a user in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. The caller cannot change its own permissions. Requires the ChangeUserPermissions permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserPermissionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
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

        await client.V1.Users[userGuid].Permissions.PutAsync(body, cancellationToken: cancellationToken);

        return $"User '{userGuid}' permissions updated.";
    }

    [McpServerTool(Name = "fargo_users_set_active")]
    [Description("Activate or deactivate a user. Requires the ActivateUser permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserActiveAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user.")] Guid userGuid,
        [Description("true to activate, false to deactivate.")] bool isActive,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Users[userGuid].IsActive.PutAsync(
            new ActiveStateDto { IsActive = isActive },
            cancellationToken: cancellationToken);

        return $"User '{userGuid}' {(isActive ? "activated" : "deactivated")}.";
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
