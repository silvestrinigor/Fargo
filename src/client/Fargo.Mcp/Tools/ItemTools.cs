using System.ComponentModel;
using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for working with Fargo items. Mirrors the <c>fargo items</c> CLI
/// surface.
/// </summary>
[McpServerToolType]
public static class ItemTools
{
    [McpServerTool(Name = "fargo_items_list")]
    [Description("List items, optionally paginated and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<ItemSummary>> ListItemsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of items to return per page.")] int? limit = null,
        [Description("Only return items that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Items.GetAsync(request =>
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

        return result?.Select(i => i.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_items_get")]
    [Description("Get a single item by its GUID.")]
    public static async Task<ItemSummary?> GetItemAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item.")] Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var item = await client.Items[itemGuid].GetAsync(cancellationToken: cancellationToken);

        return item?.ToSummary();
    }

    [McpServerTool(Name = "fargo_items_create")]
    [Description("Create a new item for an article, optionally placed in a parent container and partitions. Returns the GUID of the created item.")]
    public static async Task<Guid?> CreateItemAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article this item is an instance of.")] Guid articleGuid,
        [Description("The GUID of the parent item container this item is placed in, if any.")] Guid? parentItemContainerGuid = null,
        [Description("The GUIDs of the partitions to add the item to.")] Guid[]? partitionsToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ItemCreateDto
        {
            ArticleGuid = articleGuid,
            ParentItemContainerGuid = parentItemContainerGuid,
            PartitionsToAdd = partitionsToAdd?.Select(g => (Guid?)g).ToList()
        };

        return await client.Items.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_items_delete")]
    [Description("Delete an item by its GUID.")]
    public static async Task<string> DeleteItemAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item to delete.")] Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.Items[itemGuid].DeleteAsync(cancellationToken: cancellationToken);

        return $"Item '{itemGuid}' deleted.";
    }
}
