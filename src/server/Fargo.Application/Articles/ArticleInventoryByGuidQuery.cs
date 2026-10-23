using Fargo.Application.Common;

namespace Fargo.Application.Articles;

/// <summary>
/// Represents a query for retrieving article inventory information by article GUID,
/// optionally as it existed at a specific point in time.
/// This query can optionally include specific item container GUIDs and control whether descendant items are included in the results.
/// </summary>
/// <param name="ArticleGuid">The unique identifier of the article to retrieve inventory for</param>
/// <param name="InsideItemContainerGuids">Optional collection of item container GUIDs to filter the inventory by</param>
/// <param name="IncludeDescendents">Flag indicating whether descendant items should be included in the results (defaults to true)</param>
/// <param name="At">
/// The instant, in UTC, at which the inventory is evaluated. When
/// <see langword="null"/>, the current inventory is returned. When provided, the
/// inventory is reconstructed from the item containment history as it was at that
/// instant.
/// </param>
public sealed record ArticleInventoryByGuidQuery(
    Guid ArticleGuid,
    IReadOnlyCollection<Guid>? InsideItemContainerGuids = null,
    bool IncludeDescendents = true,
    DateTimeOffset? At = null
) : IQuery<ArticleInventoryDto?>;
