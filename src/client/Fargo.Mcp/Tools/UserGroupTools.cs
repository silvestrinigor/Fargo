using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for working with Fargo user groups. Mirrors the
/// <c>fargo user-group</c> CLI surface. Group membership of users is managed from
/// the user side (<c>fargo_users_update</c> with <c>userGroupsToAdd</c> /
/// <c>userGroupsToRemove</c>).
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
        [Description("GUID of the parent user group, if this group should be nested.")] Guid? parentUserGroup = null,
        [Description("Permissions to grant the group. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("GUIDs of the partitions the group belongs to (its membership).")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of the partitions the group is granted access to.")] Guid[]? partitionAccessesToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserGroupCreateDto
        {
            Nameid = nameid,
            Description = description,
            IsActive = isActive,
            ParentUserGroup = parentUserGroup,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd)
        };

        return await client.V1.UserGroups.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_usergroups_update")]
    [Description("Update an existing user group. Only the supplied fields are changed; omitted fields are left as-is. To move the group to the top level, set removeParentUserGroup instead of parentUserGroup.")]
    public static async Task<string> UpdateUserGroupAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the user group to update.")] Guid userGroupGuid,
        [Description("The new name (nameid).")] string? nameid = null,
        [Description("The new description.")] string? description = null,
        [Description("Set the active state.")] bool? isActive = null,
        [Description("GUID of the new parent user group.")] Guid? parentUserGroup = null,
        [Description("Set true to detach the group from its current parent.")] bool? removeParentUserGroup = null,
        [Description("Permissions to grant. " + ToolInput.PermissionCodes)] int[]? permissionsToAdd = null,
        [Description("Permissions to revoke (same codes as permissionsToAdd).")] int[]? permissionsToRemove = null,
        [Description("GUIDs of partitions to add to the group's membership.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of partitions to remove from the group's membership.")] Guid[]? partitionsToRemove = null,
        [Description("GUIDs of partitions to grant the group access to.")] Guid[]? partitionAccessesToAdd = null,
        [Description("GUIDs of partitions to revoke the group's access from.")] Guid[]? partitionAccessesToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new UserGroupUpdateDto
        {
            Nameid = nameid,
            Description = description,
            IsActive = isActive,
            ParentUserGroup = parentUserGroup,
            RemoveParentUserGroup = removeParentUserGroup,
            PermissionsToAdd = ToolInput.ToIntList(permissionsToAdd),
            PermissionsToRemove = ToolInput.ToIntList(permissionsToRemove),
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionsToRemove = ToolInput.ToGuidList(partitionsToRemove),
            PartitionAccessesToAdd = ToolInput.ToGuidList(partitionAccessesToAdd),
            PartitionAccessesToRemove = ToolInput.ToGuidList(partitionAccessesToRemove)
        };

        await client.V1.UserGroups[userGroupGuid].PutAsync(body, cancellationToken: cancellationToken);

        return $"User group '{userGroupGuid}' updated.";
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
