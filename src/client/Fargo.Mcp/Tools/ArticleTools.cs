using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Globalization;

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

        var result = await client.V1.Articles.GetAsync(request =>
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

        var article = await client.V1.Articles[barcode].GetAsync(cancellationToken: cancellationToken);

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
        [Description("How stock is tracked: 1 = Entity (each unit is an individual item; the default), 2 = Event (aggregate quantities recorded as movement events). Container articles are always entity-tracked.")] int? trackingMode = null,
        [Description("For container articles, the maximum mass the container can hold, as value+unit, e.g. '25kg'. Implies the article is a container.")] string? containerMaxMass = null,
        [Description("For container articles, the articles allowed inside the container, each as '<article-guid>' or '<article-guid>:<max-quantity>'. When any are given, only those articles may be inserted. The max quantity is optional and only valid for an entity-tracked article. Implies the article is a container.")] string[]? allowedArticles = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ArticleCreateDto
        {
            Name = name,
            Description = description,
            ArticleType = articleType,
            TrackingMode = trackingMode
        };

        var container = BuildContainer(containerMaxMass, allowedArticles);
        if (container is not null)
        {
            body.Container = container;
        }

        return await client.V1.Articles.PostAsync(body, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Builds the container portion of an article-create request from the
    /// compact MCP parameters, or returns <see langword="null"/> when no
    /// container constraints were supplied.
    /// </summary>
    private static ArticleContainerDto? BuildContainer(string? containerMaxMass, string[]? allowedArticles)
    {
        var hasMaxMass = !string.IsNullOrWhiteSpace(containerMaxMass);
        var hasAllowed = allowedArticles is { Length: > 0 };

        if (!hasMaxMass && !hasAllowed)
        {
            return null;
        }

        var container = new ArticleContainerDto();

        if (hasMaxMass)
        {
            var (value, unit) = ParseMeasure(containerMaxMass!);
            container.MaxMass = new ArticleContainerDto_maxMass { Value = value, Unit = unit };
        }

        if (hasAllowed)
        {
            container.AllowedArticles = allowedArticles!
                .Select(token =>
                {
                    var (articleGuid, maxQuantity) = ParseAllowedArticle(token);
                    return new ArticleContainerAllowedArticleDto
                    {
                        ArticleGuid = articleGuid,
                        MaxQuantity = maxQuantity is { } q
                            ? new ArticleContainerAllowedArticleDto.ArticleContainerAllowedArticleDto_maxQuantity { Double = q }
                            : null
                    };
                })
                .ToList();
        }

        return container;
    }

    /// <summary>
    /// Splits a measure token such as <c>25kg</c> into its numeric value and
    /// unit abbreviation.
    /// </summary>
    private static (double Value, string Unit) ParseMeasure(string token)
    {
        token = token.Trim();

        var index = 0;
        while (index < token.Length)
        {
            var c = token[index];
            var isNumeric = char.IsDigit(c) || c == '.' || c == '-' || c == '+' || c == ',';
            if (!isNumeric)
            {
                break;
            }

            index++;
        }

        var numberPart = token[..index];
        var unitPart = token[index..].Trim();

        if (index == 0 || unitPart.Length == 0 ||
            !double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
        {
            throw new ArgumentException(
                $"Invalid measure '{token}'. Expected a number followed by a unit, e.g. '25kg'.");
        }

        return (value, unitPart);
    }

    /// <summary>
    /// Parses a container allowed-article token of the form
    /// <c>&lt;article-guid&gt;</c> or <c>&lt;article-guid&gt;:&lt;max-quantity&gt;</c>.
    /// The maximum quantity is optional.
    /// </summary>
    private static (Guid ArticleGuid, double? MaxQuantity) ParseAllowedArticle(string token)
    {
        token = (token ?? string.Empty).Trim();

        var separator = token.LastIndexOf(':');

        string guidPart;
        double? maxQuantity = null;

        if (separator < 0)
        {
            guidPart = token;
        }
        else
        {
            guidPart = token[..separator].Trim();
            var quantityPart = token[(separator + 1)..].Trim();

            if (quantityPart.Length > 0)
            {
                if (!double.TryParse(quantityPart, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsed) || parsed <= 0)
                {
                    throw new ArgumentException(
                        $"Invalid allowed article maximum quantity '{quantityPart}' in '{token}'. Expected a positive number.");
                }

                maxQuantity = parsed;
            }
        }

        if (!Guid.TryParse(guidPart, out var articleGuid))
        {
            throw new ArgumentException(
                $"Invalid allowed article '{token}'. Expected '<article-guid>' or '<article-guid>:<max-quantity>'.");
        }

        return (articleGuid, maxQuantity);
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

        await client.V1.Articles[barcode].DeleteAsync(cancellationToken: cancellationToken);

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

        await client.V1.Articles[barcode].PatchAsync(body, cancellationToken: cancellationToken);

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

        var inventory = await client.V1.Articles[barcode].Inventory.GetAsync(request =>
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
