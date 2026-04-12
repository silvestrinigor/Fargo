using Fargo.Sdk;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;

namespace Fargo.Mcp.Tools;

[McpServerToolType]
public sealed class ItemTools(IEngine engine)
{
    [McpServerTool(Name = "list_items"), Description("Lists items accessible to the current user. Optionally filter by article GUID.")]
    public async Task<string> ListItems(
        [Description("GUID of the article to filter items by. Omit to list all items.")] string? articleGuid = null,
        [Description("Zero-based page index.")] int? page = null,
        [Description("Maximum number of results to return.")] int? limit = null)
    {
        try
        {
            Guid? articleGuidParsed = articleGuid is not null ? Guid.Parse(articleGuid) : null;
            var items = await engine.Items.GetManyAsync(articleGuid: articleGuidParsed, page: page, limit: limit);
            var results = items.Select(i => new { i.Guid, i.ArticleGuid }).ToList();
            foreach (var i in items)
            {
                await i.DisposeAsync();
            }

            return JsonSerializer.Serialize(results);
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "get_item"), Description("Gets a single item by its GUID.")]
    public async Task<string> GetItem(
        [Description("The GUID of the item.")] string itemGuid)
    {
        try
        {
            await using var item = await engine.Items.GetAsync(Guid.Parse(itemGuid));
            return JsonSerializer.Serialize(new { item.Guid, item.ArticleGuid });
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "create_item"), Description("Creates a new item as an instance of the specified article.")]
    public async Task<string> CreateItem(
        [Description("The GUID of the article this item is an instance of.")] string articleGuid,
        [Description("GUID of the partition to associate with the item. Omit to use the global partition.")] string? partitionGuid = null)
    {
        try
        {
            Guid? firstPartition = partitionGuid is not null ? Guid.Parse(partitionGuid) : null;
            await using var item = await engine.Items.CreateAsync(Guid.Parse(articleGuid), firstPartition);
            return $"Created item with GUID: {item.Guid}";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }

    [McpServerTool(Name = "delete_item"), Description("Deletes an item.")]
    public async Task<string> DeleteItem(
        [Description("The GUID of the item to delete.")] string itemGuid)
    {
        try
        {
            await engine.Items.DeleteAsync(Guid.Parse(itemGuid));
            return "Item deleted successfully.";
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}
