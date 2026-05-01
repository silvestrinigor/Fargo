using Fargo.Api.Users;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class UserTools(IUserManager users)
{
    [McpServerTool(Name = "list_users"), Description("Lists users accessible to the current user.")]
    public async Task<string> ListUsers(
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null)
    {
        try
        {
            var result = await users.GetManyAsync(page: page, limit: limit);
            var list = result.Select(u => new
            {
                u.Guid,
                u.Nameid,
                u.FirstName,
                u.LastName,
                u.Description,
                u.IsActive,
                u.PartitionAccesses
            }).ToList();
            foreach (var u in result)
            {
                await u.DisposeAsync();
            }

            return JsonSerializer.Serialize(list);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_user"), Description("Gets a single user by their GUID.")]
    public async Task<string> GetUser(
        [Description("The GUID of the user.")] string userGuid)
    {
        try
        {
            await using var user = await users.GetAsync(Guid.Parse(userGuid));
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
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
