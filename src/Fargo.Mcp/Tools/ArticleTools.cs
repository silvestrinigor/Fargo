using Fargo.ClientHttp;
using Fargo.ClientHttp.Models;
using Fargo.Mcp.Authentication;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace Fargo.Mcp.Tools;

/// <summary>
/// Only reading supports addressing an article by barcode; update, delete and inventory are addressed by
/// GUID (the server exposes no barcode route for them).
/// </summary>
[McpServerToolType]
public static class ArticleTools
{

    [McpServerTool(Name = "fargo_articles_list")]
    [Description("List articles, optionally paginated, searched, sorted and filtered to those under specific partitions.")]
    public static async Task<IReadOnlyList<ArticleSummary>> ListArticlesAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("Page number to retrieve, starting at 1.")] int? page = null,
        [Description("Maximum number of articles to return per page.")] int? limit = null,
        [Description("Only return articles that belong to any of these partition GUIDs.")] Guid[]? partitions = null,
        [Description("Case-insensitive text to match against the article name or description.")] string? search = null,
        [Description("Sort field, optionally prefixed with '-' for descending (e.g. name, -name). Fields: guid, name, description, type, trackingmode.")] string? sort = null,
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
                    [.. partitions.Select(g => (Guid?)g)];
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                request.QueryParameters.Search = search;
            }

            if (!string.IsNullOrWhiteSpace(sort))
            {
                request.QueryParameters.Sort = sort;
            }
        }, cancellationToken);

        return result?.Items?.Select(a => a.ToSummary()).ToArray() ?? [];
    }

    [McpServerTool(Name = "fargo_articles_get")]
    [Description("Get a single article by its GUID.")]
    public static async Task<ArticleSummary?> GetArticleAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article.")] Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var article = await client.V1.Articles[articleGuid.ToString()].GetAsync(cancellationToken: cancellationToken);

        return article?.ToSummary();
    }

    [McpServerTool(Name = "fargo_articles_get_by_barcode")]
    [Description("Get a single article by one of its barcodes, given as {value}:{type} (e.g. 1234567890128:ean13). Use this to find an article's GUID, which the update, delete and inventory tools require.")]
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
    [Description("Create a new article. The article type is inferred from the options given: variationFromArticleGuid = variation; packFromArticleGuid and/or packQuantity = pack; kitComponents = kit; isContainer, containerMaxMass or allowedArticles = container; none = a plain article. These are mutually exclusive. Returns the GUID of the created article.")]
    public static async Task<Guid?> CreateArticleAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The name of the article.")] string name,
        [Description("An optional description of the article.")] string? description = null,
        [Description("Id of the tracking mode (see fargo_metadata_get, catalog 'article-tracking-modes'). Defaults to Entity, where each unit is an individual item; Event records aggregate quantities as movement events. Container articles are always entity-tracked.")] int? trackingMode = null,
        [Description("Optional shelf life as an ISO-8601 duration, e.g. 'P30D'.")] string? shelfLife = null,
        [Description("Optional color, e.g. a hex code like '#FF0000'.")] string? color = null,
        [Description("Optional mass as value+unit, e.g. '2.5kg'. Units: mg, g, kg, oz, lb.")] string? mass = null,
        [Description("Optional dimension along X as value+unit, e.g. '10cm'. Units: mm, cm, m, in, ft.")] string? lengthX = null,
        [Description("Optional dimension along Y as value+unit, e.g. '5cm'.")] string? lengthY = null,
        [Description("Optional dimension along Z as value+unit, e.g. '2cm'.")] string? lengthZ = null,
        [Description("Optional EAN-13 barcode to assign to the article.")] string? barcodeEan13 = null,
        [Description("GUID of the source article this is a variation of. Implies a variation article.")] Guid? variationFromArticleGuid = null,
        [Description("GUID of the source article this pack is composed of. Implies a pack article.")] Guid? packFromArticleGuid = null,
        [Description("Quantity of the source article contained in the pack. Implies a pack article.")] double? packQuantity = null,
        [Description("Kit components, each as '<article-guid>' or '<article-guid>:<quantity>'. Implies a kit article.")] string[]? kitComponents = null,
        [Description("Create the article as a container.")] bool isContainer = false,
        [Description("For container articles, the maximum mass the container can hold, as value+unit, e.g. '25kg'. Implies a container article.")] string? containerMaxMass = null,
        [Description("For container articles, the articles allowed inside, each as '<article-guid>' or '<article-guid>:<max-quantity>'. When any are given, only those articles may be inserted. The max quantity is optional and only valid for an entity-tracked article. Implies a container article.")] string[]? allowedArticles = null,
        [Description("GUIDs of the partitions to associate the article with.")] Guid[]? partitionsToAdd = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var articleTypeName = InferArticleType(
            variationPresent: variationFromArticleGuid is not null,
            packPresent: packFromArticleGuid is not null || packQuantity is not null,
            kitPresent: kitComponents is { Length: > 0 },
            containerPresent: isContainer || !string.IsNullOrWhiteSpace(containerMaxMass) || allowedArticles is { Length: > 0 });

        var articleType = await MetadataIds.ResolveAsync(client.V1.Metadata.ArticleTypes.GetAsync, articleTypeName, cancellationToken);

        var resolvedTrackingMode = trackingMode
            ?? await MetadataIds.ResolveAsync(client.V1.Metadata.ArticleTrackingModes.GetAsync, "Entity", cancellationToken);

        if (articleTypeName == "Container"
            && resolvedTrackingMode == await MetadataIds.ResolveAsync(client.V1.Metadata.ArticleTrackingModes.GetAsync, "Event", cancellationToken))
        {
            throw new ArgumentException("Container articles are always entity-tracked; the Event tracking mode cannot be combined with a container.");
        }

        var body = new ArticleCreateDto
        {
            Name = name,
            Description = description,
            ArticleType = articleType,
            TrackingMode = resolvedTrackingMode,
            ShelfLife = shelfLife,
            Color = color,
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd)
        };

        if (!string.IsNullOrWhiteSpace(mass))
        {
            var (value, unit) = ToolInput.ParseMeasure(mass, nameof(mass));
            body.Mass = new ArticleCreateDto_mass { Value = value, Unit = unit };
        }

        var dimension = BuildDimension(lengthX, lengthY, lengthZ);
        if (dimension is not null)
        {
            body.Dimension = dimension;
        }

        if (!string.IsNullOrWhiteSpace(barcodeEan13))
        {
            body.Barcode = new ArticleBarcodeDto { Ean13 = barcodeEan13 };
        }

        if (variationFromArticleGuid is not null)
        {
            body.Variation = new ArticleVariationDto { FromArticleGuid = variationFromArticleGuid };
        }

        if (packFromArticleGuid is not null || packQuantity is not null)
        {
            body.Pack = new ArticlePackDto
            {
                FromArticleGuid = packFromArticleGuid,
                Quantity = packQuantity is { } q ? new Scalar { Double = q } : null
            };
        }

        if (kitComponents is { Length: > 0 })
        {
            body.KitComponents = [.. kitComponents
                .Select(token =>
                {
                    var (articleGuid, quantity) = ToolInput.ParseKitComponent(token);
                    return new ArticleKitComponentDto
                    {
                        ArticleGuid = articleGuid,
                        Quantity = quantity is { } q ? new Scalar { Double = q } : null
                    };
                })];
        }

        var container = BuildContainer(containerMaxMass, allowedArticles);
        if (container is not null)
        {
            body.Container = container;
        }

        return await client.V1.Articles.PostAsync(body, cancellationToken: cancellationToken);
    }

    [McpServerTool(Name = "fargo_articles_delete")]
    [Description("Delete an article by its GUID. To delete by barcode, first resolve the GUID with fargo_articles_get_by_barcode.")]
    public static async Task<string> DeleteArticleAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article to delete.")] Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        await client.V1.Articles[articleGuid.ToString()].DeleteAsync(cancellationToken: cancellationToken);

        return $"Article '{articleGuid}' deleted.";
    }

    [McpServerTool(Name = "fargo_articles_update")]
    [Description("Update an existing article by its GUID. Only the supplied fields are changed; omitted fields are left as-is. To clear a mass, shelf life, dimension or barcode, use the matching remove flag. To update by barcode, first resolve the GUID with fargo_articles_get_by_barcode.")]
    public static async Task<string> UpdateArticleAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article to update.")] Guid articleGuid,
        [Description("The new name of the article. Leave null to keep the existing name.")] string? name = null,
        [Description("The new description of the article. Leave null to keep the existing description.")] string? description = null,
        [Description("The new shelf life as an ISO-8601 duration, e.g. 'P30D'.")] string? shelfLife = null,
        [Description("Set true to clear the shelf life.")] bool? removeShelfLife = null,
        [Description("The new mass as value+unit, e.g. '2.5kg'.")] string? mass = null,
        [Description("Set true to clear the mass.")] bool? removeMass = null,
        [Description("The new dimension along X as value+unit, e.g. '10cm'.")] string? lengthX = null,
        [Description("Set true to clear the X dimension.")] bool? removeLengthX = null,
        [Description("The new dimension along Y as value+unit, e.g. '5cm'.")] string? lengthY = null,
        [Description("Set true to clear the Y dimension.")] bool? removeLengthY = null,
        [Description("The new dimension along Z as value+unit, e.g. '2cm'.")] string? lengthZ = null,
        [Description("Set true to clear the Z dimension.")] bool? removeLengthZ = null,
        [Description("The new EAN-13 barcode.")] string? barcodeEan13 = null,
        [Description("Set true to clear the EAN-13 barcode.")] bool? removeBarcodeEan13 = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new ArticleUpdateDto
        {
            Name = name,
            Description = description,
            ShelfLife = shelfLife,
            RemoveShelfLife = removeShelfLife,
            RemoveMass = removeMass
        };

        if (!string.IsNullOrWhiteSpace(mass))
        {
            var (value, unit) = ToolInput.ParseMeasure(mass, nameof(mass));
            body.Mass = new ArticleUpdateDto_mass { Value = value, Unit = unit };
        }

        var dimension = BuildDimensionUpdate(lengthX, removeLengthX, lengthY, removeLengthY, lengthZ, removeLengthZ);
        if (dimension is not null)
        {
            body.Dimension = dimension;
        }

        if (!string.IsNullOrWhiteSpace(barcodeEan13) || removeBarcodeEan13 is not null)
        {
            body.Barcode = new ArticleBarcodeUpdateDto
            {
                Ean13 = string.IsNullOrWhiteSpace(barcodeEan13) ? null : barcodeEan13,
                RemoveEan13 = removeBarcodeEan13
            };
        }

        await client.V1.Articles[articleGuid.ToString()].PatchAsync(body, cancellationToken: cancellationToken);

        return $"Article '{articleGuid}' updated.";
    }

    [McpServerTool(Name = "fargo_articles_set_partitions")]
    [Description("Change the partitions an article belongs to in one request. At least one partition is required and none may be in both lists. The caller needs access to every partition it touches. Requires the ChangeArticlePartitions permission, which is separate from the permission to update.")]
    public static async Task<string> SetArticlePartitionsAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article.")] Guid articleGuid,
        [Description("GUIDs of partitions to add.")] Guid[]? partitionsToAdd = null,
        [Description("GUIDs of partitions to remove.")] Guid[]? partitionsToRemove = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var body = new PartitionMembershipsChangeDto
        {
            PartitionsToAdd = ToolInput.ToGuidList(partitionsToAdd),
            PartitionsToRemove = ToolInput.ToGuidList(partitionsToRemove)
        };

        await client.V1.Articles[articleGuid.ToString()].Partitions.PutAsync(body, cancellationToken: cancellationToken);

        return $"Article '{articleGuid}' partitions updated.";
    }

    [McpServerTool(Name = "fargo_articles_get_inventory")]
    [Description("Get inventory information (total item count) for an article by its GUID. When 'at' is supplied, the count is reconstructed from the item containment history as it was at that instant.")]
    public static async Task<ArticleInventorySummary?> GetArticleInventoryAsync(
        FargoApiClient client,
        FargoAuthenticator authenticator,
        [Description("The GUID of the article.")] Guid articleGuid,
        [Description("Only count items inside these container item GUIDs. When omitted, all items of the article are counted.")] Guid[]? insideItemContainerGuids = null,
        [Description("Include items in nested containers of the given containers. Defaults to true. Ignored when no containers are given.")] bool? includeDescendents = null,
        [Description("Optional instant (ISO 8601 UTC, e.g. 2026-03-01T12:00:00Z) at which to evaluate the inventory. When omitted, the current inventory is returned.")] DateTimeOffset? at = null,
        CancellationToken cancellationToken = default)
    {
        await authenticator.EnsureAuthenticatedAsync(cancellationToken);

        var inventory = await client.V1.Articles[articleGuid.ToString()].Inventory.GetAsync(request =>
        {
            if (insideItemContainerGuids is { Length: > 0 })
            {
                request.QueryParameters.InsideItemContainerGuids =
                    [.. insideItemContainerGuids.Select(g => (Guid?)g)];
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

    private static string InferArticleType(bool variationPresent, bool packPresent, bool kitPresent, bool containerPresent)
    {
        var signals = new (bool Present, string Type, string Hint)[]
        {
            (variationPresent, "Variation", "variationFromArticleGuid"),
            (packPresent, "Pack", "packFromArticleGuid / packQuantity"),
            (kitPresent, "Kit", "kitComponents"),
            (containerPresent, "Container", "isContainer / containerMaxMass / allowedArticles"),
        };

        var present = signals.Where(s => s.Present).ToList();

        if (present.Count > 1)
        {
            throw new ArgumentException(
                $"Conflicting article type options: {string.Join(", ", present.Select(s => s.Hint))}. "
                + "An article can only be one of variation, pack, kit, or container. Supply the options for a single type.");
        }

        return present.Count == 1 ? present[0].Type : "Default";
    }

    private static ArticleDimensionDto? BuildDimension(string? lengthX, string? lengthY, string? lengthZ)
    {
        var hasX = !string.IsNullOrWhiteSpace(lengthX);
        var hasY = !string.IsNullOrWhiteSpace(lengthY);
        var hasZ = !string.IsNullOrWhiteSpace(lengthZ);

        if (!hasX && !hasY && !hasZ)
        {
            return null;
        }

        var dimension = new ArticleDimensionDto();

        if (hasX)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthX!, nameof(lengthX));
            dimension.LengthX = new ArticleDimensionDto_lengthX { Value = value, Unit = unit };
        }

        if (hasY)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthY!, nameof(lengthY));
            dimension.LengthY = new ArticleDimensionDto_lengthY { Value = value, Unit = unit };
        }

        if (hasZ)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthZ!, nameof(lengthZ));
            dimension.LengthZ = new ArticleDimensionDto_lengthZ { Value = value, Unit = unit };
        }

        return dimension;
    }

    private static ArticleDimensionUpdateDto? BuildDimensionUpdate(
        string? lengthX, bool? removeLengthX,
        string? lengthY, bool? removeLengthY,
        string? lengthZ, bool? removeLengthZ)
    {
        var hasX = !string.IsNullOrWhiteSpace(lengthX);
        var hasY = !string.IsNullOrWhiteSpace(lengthY);
        var hasZ = !string.IsNullOrWhiteSpace(lengthZ);

        if (!hasX && !hasY && !hasZ && removeLengthX is null && removeLengthY is null && removeLengthZ is null)
        {
            return null;
        }

        var dimension = new ArticleDimensionUpdateDto
        {
            RemoveLengthX = removeLengthX,
            RemoveLengthY = removeLengthY,
            RemoveLengthZ = removeLengthZ
        };

        if (hasX)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthX!, nameof(lengthX));
            dimension.LengthX = new ArticleDimensionUpdateDto_lengthX { Value = value, Unit = unit };
        }

        if (hasY)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthY!, nameof(lengthY));
            dimension.LengthY = new ArticleDimensionUpdateDto_lengthY { Value = value, Unit = unit };
        }

        if (hasZ)
        {
            var (value, unit) = ToolInput.ParseMeasure(lengthZ!, nameof(lengthZ));
            dimension.LengthZ = new ArticleDimensionUpdateDto_lengthZ { Value = value, Unit = unit };
        }

        return dimension;
    }

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
            var (value, unit) = ToolInput.ParseMeasure(containerMaxMass!, nameof(containerMaxMass));
            container.MaxMass = new ArticleContainerDto_maxMass { Value = value, Unit = unit };
        }

        if (hasAllowed)
        {
            container.AllowedArticles = [.. allowedArticles!
                .Select(token =>
                {
                    var (articleGuid, maxQuantity) = ToolInput.ParseAllowedArticle(token);
                    return new ArticleContainerAllowedArticleDto
                    {
                        ArticleGuid = articleGuid,
                        MaxQuantity = maxQuantity is { } q
                            ? new ArticleContainerAllowedArticleDto.ArticleContainerAllowedArticleDto_maxQuantity { Double = q }
                            : null
                    };
                })];
        }

        return container;
    }
}
