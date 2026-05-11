using Fargo.Sdk.Users;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class UserTools(IUserClient users)
{
    [McpServerTool(Name = "list_users"), Description("Lists users accessible to the current user. Optionally filters by partition and public users.")]
    public async Task<string> ListUsers(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null,
        [Description("Partition GUIDs whose direct child users should be included. Omit for no child filter.")] string[]? childOfAnyOfThesePartitions = null,
        [Description("When true, include public users with no partition.")] bool? notChildOfAnyPartition = null)
    {
        var partitionGuids = childOfAnyOfThesePartitions?.Select(Guid.Parse).ToArray();
        var response = await users.GetManyAsync(
            page: page,
            limit: limit,
            childOfAnyOfThesePartitions: partitionGuids,
            notChildOfAnyPartition: notChildOfAnyPartition);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Result!.Select(u => new
        {
            u.Guid,
            u.Nameid,
            u.FirstName,
            u.LastName,
            u.Description,
            u.IsActive,
            u.PartitionAccesses
        }).ToList();

        return JsonSerializer.Serialize(list);
    }

    [McpServerTool(Name = "get_user"), Description("Gets a single user by their GUID.")]
    public async Task<string> GetUser(
        [Description("The GUID of the user.")] string userGuid)
    {
        var response = await users.GetAsync(Guid.Parse(userGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var user = response.Result!;
        return JsonSerializer.Serialize(new
        {
            user.Guid,
            user.Nameid,
            user.FirstName,
            user.LastName,
            user.Description,
            user.IsActive,
            user.PartitionAccesses,
            user.Permissions
        });
    }
}
