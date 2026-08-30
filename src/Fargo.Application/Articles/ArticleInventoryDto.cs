namespace Fargo.Application.Articles;

/// <summary>
/// Represents a summary of inventory information for a specific article.
/// </summary>
/// <param name="TotalCount">
/// The total number of items available in inventory for this article.
/// </param>
public sealed record ArticleInventoryDto(
    int TotalCount
);
