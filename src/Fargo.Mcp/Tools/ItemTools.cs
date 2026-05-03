using Fargo.Api.Items;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ItemTools(IItemHttpClient items)
{
    [McpServerTool(Name = "list_items"), Description("Lists items accessible to the current user. Optionally filter by article GUID.")]
    public async Task<string> ListItems(
        [Description("GUID of the article to filter items by. Omit to list all items.")] string? articleGuid = null,
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null)
    {
        Guid? articleGuidParsed = articleGuid is not null ? Guid.Parse(articleGuid) : null;
        var response = await items.GetManyAsync(articleGuid: articleGuidParsed, page: page, limit: limit);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var list = response.Data!.Select(i => new { i.Guid, i.ArticleGuid }).ToList();
        return JsonSerializer.Serialize(list);
    }

    [McpServerTool(Name = "get_item"), Description("Gets a single item by its GUID.")]
    public async Task<string> GetItem(
        [Description("The GUID of the item.")] string itemGuid)
    {
        var response = await items.GetAsync(Guid.Parse(itemGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        var item = response.Data!;
        return JsonSerializer.Serialize(new { item.Guid, item.ArticleGuid });
    }

    [McpServerTool(Name = "create_item"), Description("Creates a new item as an instance of the specified article.")]
    public async Task<string> CreateItem(
        [Description("The GUID of the article this item is an instance of.")] string articleGuid,
        [Description("GUID of the partition to associate with the item. Omit to create a public item with no partition.")] string? partitionGuid = null)
    {
        Guid? firstPartition = partitionGuid is not null ? Guid.Parse(partitionGuid) : null;
        var response = await items.CreateAsync(Guid.Parse(articleGuid), firstPartition);
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return $"Created item with GUID: {response.Data}";
    }

    [McpServerTool(Name = "delete_item"), Description("Deletes an item.")]
    public async Task<string> DeleteItem(
        [Description("The GUID of the item to delete.")] string itemGuid)
    {
        var response = await items.DeleteAsync(Guid.Parse(itemGuid));
        if (!response.IsSuccess)
        {
            return $"Error: {response.Error!.Detail}";
        }

        return "Item deleted successfully.";
    }
}
