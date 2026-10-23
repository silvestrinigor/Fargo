using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// MCP tools for working with Fargo articles. Mirrors the <c>fargo article</c>
/// CLI surface.
/// </summary>
[McpServerToolType]
public static class ArticleTools
{
    [McpServerTool(Name = "fargo_articles_list")]
    [Description("List articles, optionally paginated and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<ArticleSummary>> ListArticlesAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of articles to return per page.")] int? limit = null,
        [Description("Only return articles that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var result = await client.Articles.GetAsync(request =>
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

        return result?.Items?.Select(a => a.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_articles_get_by_barcode")]
    [Description("Get a single article by one of its barcodes, given as {value}:{type} (e.g. 1234567890128:ean13).")]
    public static async Task<ArticleSummary?> GetArticleByBarcodeAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The barcode in {value}:{type} format, e.g. 1234567890128:ean13.")] string barcode,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var article = await client.Articles[barcode].GetAsync(cancellationToken: cancellationToken);

        return article?.ToSummary();
    }

    [McpServerTool(Name = "fargo_articles_create")]
    [Description("Create a new article. Returns the GUID of the created article.")]
    public static async Task<Guid?> CreateArticleAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The name of the article.")] string name,
        [Description("An optional description of the article.")] string? description = null,
        [Description("The numeric article type. Defaults to the API's default when omitted.")] int? articleType = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ArticleCreateDto
        {
            Name = name,
            Description = description,
            ArticleType = articleType
        };

        return await client.Articles.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_articles_delete_by_barcode")]
    [Description("Delete an article identified by one of its barcodes, given as {value}:{type}.")]
    public static async Task<string> DeleteArticleByBarcodeAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The barcode in {value}:{type} format, e.g. 1234567890128:ean13.")] string barcode,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.Articles[barcode].DeleteAsync(cancellationToken: cancellationToken);

        return $"Article with barcode '{barcode}' deleted.";
    }

    [McpServerTool(Name = "fargo_articles_update_by_barcode")]
    [Description("Update an existing article identified by one of its barcodes. Only the supplied fields are changed; omitted fields are left as-is.")]
    public static async Task<string> UpdateArticleByBarcodeAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The barcode in {value}:{type} format, e.g. 1234567890128:ean13.")] string barcode,
        [Description("The new name of the article. Leave null to keep the existing name.")] string? name = null,
        [Description("The new description of the article. Leave null to keep the existing description.")] string? description = null,
        [Description("The GUIDs of the partitions to associate with the article.")] Guid[]? partitionsToAdd = null,
        [Description("The GUIDs of the partitions to disassociate from the article.")] Guid[]? partitionsToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ArticleUpdateDto
        {
            Name = name,
            Description = description,
            PartitionsToAdd = partitionsToAdd?.Select(g => (Guid?)g).ToList(),
            PartitionsToRemove = partitionsToRemove?.Select(g => (Guid?)g).ToList()
        };

        await client.Articles[barcode].PatchAsync(body, cancellationToken: cancellationToken);

        return $"Article with barcode '{barcode}' updated.";
    }

    [McpServerTool(Name = "fargo_articles_get_inventory_by_barcode")]
    [Description("Get inventory information (total item count) for an article identified by one of its barcodes. When 'at' is supplied, the count is reconstructed from the item containment history as it was at that instant.")]
    public static async Task<ArticleInventorySummary?> GetArticleInventoryByBarcodeAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The barcode in {value}:{type} format, e.g. 1234567890128:ean13.")] string barcode,
        [Description("Only count items inside these container item GUIDs. When omitted, all items of the article are counted.")] Guid[]? insideItemContainerGuids = null,
        [Description("Include items in nested containers of the given containers. Defaults to true. Ignored when no containers are given.")] bool? includeDescendents = null,
        [Description("Optional instant (ISO 8601 UTC, e.g. 2026-03-01T12:00:00Z) at which to evaluate the inventory. When omitted, the current inventory is returned.")] DateTimeOffset? at = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var inventory = await client.Articles[barcode].Inventory.GetAsync(request =>
        {
            if (insideItemContainerGuids is { Length: > 0 })
            {
                request.QueryParameters.InsideItemContainerGuids =
                    insideItemContainerGuids.Select(g => (Guid?)g).ToArray();
            }

            if (includeDescendents.HasValue)
            {
                request.QueryParameters.IncludeDescendents = includeDescendents.Value;
            }

            if (at.HasValue)
            {
                request.QueryParameters.At = at.Value;
            }
        }, cancellationToken);

        return inventory?.ToSummary();
    }
}
