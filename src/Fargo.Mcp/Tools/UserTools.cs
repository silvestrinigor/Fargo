using Fargo.Sdk.Users;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class UserTools(IUserHttpClient users)
{
    [McpServerTool(Name = "list_users"), Description("Lists users accessible to the current user. Optionally filters by partition and public users.")]
    public async Task<string> ListUsers(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null,
        [Description("Partition GUIDs to include. Omit for no partition filter.")] string[]? insideAnyOfThisPartitions = null,
        [Description("When true, include public users with no partition.")] bool? notInsideAnyPartition = null)
    {
        var partitionGuids = insideAnyOfThisPartitions?.Select(Guid.Parse).ToArray();
        var response = await users.GetManyAsync(
            page: page,
            limit: limit,
            insideAnyOfThisPartitions: partitionGuids,
            notInsideAnyPartition: notInsideAnyPartition);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Data!.Select(u => new
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

        var user = response.Data!;
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
