using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for working with Fargo items. Mirrors the <c>fargo item</c> CLI
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

        return result?.Items?.Select(i => i.ToSummary()).ToArray() ?? [];
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

    [McpServerTool(Name = "fargo_items_update")]
    [Description("Update an existing item: move it to (or out of) a parent container and adjust its partitions. Only the supplied changes are applied.")]
    public static async Task<string> UpdateItemAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item to update.")] Guid itemGuid,
        [Description("The GUID of the parent item container to move this item into. Leave null to keep the current container.")] Guid? parentItemContainerGuid = null,
        [Description("When true, removes the item from its current parent container. Takes precedence over parentItemContainerGuid.")] bool? removeFromParentItemContainer = null,
        [Description("The GUIDs of the partitions to add the item to.")] Guid[]? partitionsToAdd = null,
        [Description("The GUIDs of the partitions to remove the item from.")] Guid[]? partitionsToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ItemUpdateDto
        {
            ParentItemContainerGuid = parentItemContainerGuid,
            RemoveFromParentItemContainer = removeFromParentItemContainer,
            PartitionsToAdd = partitionsToAdd?.Select(g => (Guid?)g).ToList(),
            PartitionsToRemove = partitionsToRemove?.Select(g => (Guid?)g).ToList()
        };

        await client.Items[itemGuid].PatchAsync(body, cancellationToken: cancellationToken);

        return $"Item '{itemGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_items_get_location")]
    [Description("Get the location of an item: the chain of parent container items from the item up to its root container. When 'asOf' is supplied, the location is reconstructed from the item's containment history as it was at that instant.")]
    public static async Task<IReadOnlyList<ItemSummary>> GetItemLocationAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item.")] Guid itemGuid,
        [Description("Optional instant (ISO 8601 UTC, e.g. 2026-03-01T12:00:00Z) at which to evaluate the location. When omitted, the current location is returned.")] DateTimeOffset? asOf = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Items[itemGuid].Location.GetAsync(request =>
        {
            if (asOf.HasValue)
            {
                request.QueryParameters.AsOf = asOf.Value;
            }
        }, cancellationToken);

        return result?.Select(i => i.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_items_list_moviments")]
    [Description("List the movement history of an item: each time it was moved between containers and when.")]
    public static async Task<IReadOnlyList<ItemMovimentSummary>> ListItemMovimentsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item.")] Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Items[itemGuid].Moviments.GetAsync(cancellationToken: cancellationToken);

        return result?.Select(m => m.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_items_get_inventory")]
    [Description("Get the inventory of an item treated as a container: the count of contained items grouped by article.")]
    public static async Task<IReadOnlyList<ItemContainerInventorySummary>> GetItemInventoryAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the item (container).")] Guid itemGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Items[itemGuid].Inventory.GetAsync(cancellationToken: cancellationToken);

        return result?.Select(i => i.ToSummary()).ToArray() ?? [];
    }
}
