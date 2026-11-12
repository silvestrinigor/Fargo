using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Group membership of users is managed from the user side (<c>fargo_users_update</c> with
/// <c>userGroupsToAdd</c> / <c>userGroupsToRemove</c>).
/// </summary>
[McpServerToolType]
public static class UserGroupTools
{
    [McpServerTool(Name = "fargo_usergroups_list")]
    [Description("List user groups, optionally paginated and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<UserGroupSummary>> ListUserGroupsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of user groups to return per page.")] int? limit = null,
        [Description("Only return user groups that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        [Description("Case-insensitive text to match against the user group nameid or description.")] string? search = null,
        [Description("Sort field, optionally prefixed with '-' for descending (e.g. nameid, -nameid). Fields: guid, nameid, description, isactive.")] string? sort = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.V1.UserGroups.GetAsync(request =>
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

        return result?.Items?.Select(g => g.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_usergroups_get")]
    [Description("Get a single user group by its GUID.")]
    public static async Task<UserGroupSummary?> GetUserGroupAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group.")] Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var group = await client.V1.UserGroups[userGroupGuid].GetAsync(cancellationToken: cancellationToken);

        return group?.ToSummary();
    }

    [McpServerTool(Name = "fargo_usergroups_create")]
    [Description("Create a new user group, optionally nested under a parent group. Returns the GUID of the created user group.")]
    public static async Task<Guid?> CreateUserGroupAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Unique name (nameid) of the user group.")] string nameid,
        [Description("Optional description of the user group.")] string? description = null,
        [Description("Whether the group is active. Defaults to the server default (active).")] bool? isActive = null,
        [Description("GUID of the parent user group, if this group should be nested. Requires the ChangeUserGroupParent permission, and the caller must hold the permissions of the parent and its ancestors.")] Guid? parentUserGroup = null,
        [Description("Permissions to grant the group; requires the ChangeUserGroupPermissions permission. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("GUIDs of the partitions the group belongs to (its membership). Requires the ChangeUserGroupPartitions permission.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of the partitions the group is granted access to. Requires the ChangeUserGroupPartitions permission.")] Guid[]? partitionAccessesToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserGroupCreateDto
        {
            Nameid = nameid,
            Description = description,
            ParentUserGroup = parentUserGroup,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd)
        };

        return await client.V1.UserGroups.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_usergroups_update")]
    [Description("Update the name or description of an existing user group. Only the supplied fields are changed; omitted fields are left as-is. Activation, permissions, partitions and the parent group have their own tools.")]
    public static async Task<string> UpdateUserGroupAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group to update.")] Guid userGroupGuid,
        [Description("The new name (nameid).")] string? nameid = null,
        [Description("The new description.")] string? description = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserGroupUpdateDto
        {
            Nameid = nameid,
            Description = description
        };

        await client.V1.UserGroups[userGroupGuid].PutAsync(body, cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' updated.";
    }
    [McpServerTool(Name = "fargo_usergroups_set_partitions")]
    [Description("Change the partitions of a user group in one request: the partitions it belongs to (membership) and the partitions it can access. At least one list is required and no partition may be in both the add and remove list of the same kind. The caller needs access to every partition it touches. Requires the ChangeUserGroupPartitions permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserGroupPartitionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the a user group.")] Guid userGroupGuid,
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

        await client.V1.UserGroups[userGroupGuid].Partitions.PutAsync(body, cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' partitions updated.";
    }
    [McpServerTool(Name = "fargo_usergroups_set_parent")]
    [Description("Place a user group under a parent group. Members of the group inherit the permissions of the parent and of its ancestors, so the caller must hold all of them. Rejects an assignment that would create a cycle. Requires the ChangeUserGroupParent permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserGroupParentAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group.")] Guid userGroupGuid,
        [Description("The GUID of the new parent user group.")] Guid parentUserGroupGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.UserGroups[userGroupGuid].Parent.PutAsync(
            new ParentUserGroupDto { ParentUserGroupGuid = parentUserGroupGuid },
            cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' placed under '{parentUserGroupGuid}'.";
    }

    [McpServerTool(Name = "fargo_usergroups_remove_parent")]
    [Description("Detach a user group from its parent, so its members stop inheriting the parent's permissions. A group without a parent is left as it is. Requires the ChangeUserGroupParent permission.")]
    public static async Task<string> RemoveUserGroupParentAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group.")] Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.UserGroups[userGroupGuid].Parent.DeleteAsync(cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' detached from its parent.";
    }
    [McpServerTool(Name = "fargo_usergroups_set_permissions")]
    [Description("Grant and revoke permissions of a user group in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. Requires the ChangeUserGroupPermissions permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserGroupPermissionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group.")] Guid userGroupGuid,
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

        await client.V1.UserGroups[userGroupGuid].Permissions.PutAsync(body, cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' permissions updated.";
    }

    [McpServerTool(Name = "fargo_usergroups_set_active")]
    [Description("Activate or deactivate a user group. Requires the ActivateUserGroup permission, which is separate from the permission to update.")]
    public static async Task<string> SetUserGroupActiveAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group.")] Guid userGroupGuid,
        [Description("true to activate, false to deactivate.")] bool isActive,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.UserGroups[userGroupGuid].IsActive.PutAsync(
            new ActiveStateDto { IsActive = isActive },
            cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' {(isActive ? "activated" : "deactivated")}.";
    }

    [McpServerTool(Name = "fargo_usergroups_delete")]
    [Description("Delete a user group by its GUID. This cannot be undone.")]
    public static async Task<string> DeleteUserGroupAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group to delete.")] Guid userGroupGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.UserGroups[userGroupGuid].DeleteAsync(cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' deleted.";
    }
}
