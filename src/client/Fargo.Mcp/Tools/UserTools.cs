using Fargo.ClientHttp;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for reading Fargo users. Exposes read-only access (list/get); user
/// mutation requires credentials the agent is not expected to manage.
/// </summary>
[McpServerToolType]
public static class UserTools
{
    [McpServerTool(Name = "fargo_users_list")]
    [Description("List users, optionally paginated and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<UserSummary>> ListUsersAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of users to return per page.")] int? limit = null,
        [Description("Only return users that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Users.GetAsync(request =>
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

        var user = await client.Users[userGuid].GetAsync(cancellationToken: cancellationToken);

        return user?.ToSummary();
    }
}
