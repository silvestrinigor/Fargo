using System.ComponentModel;
using Fargo.ClientHttp;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for reading Fargo user groups. Exposes read-only access
/// (list/get).
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
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.UserGroups.GetAsync(request =>
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
        }, cancellationToken);

        return result?.Select(g => g.ToSummary()).ToArray() ?? [];
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

        var group = await client.UserGroups[userGroupGuid].GetAsync(cancellationToken: cancellationToken);

        return group?.ToSummary();
    }
}
