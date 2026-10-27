namespace Fargo.Application.Articles;

/// <summary>
/// Represents a summary of inventory information for a specific article.
/// </summary>
/// <param name="TotalCount">
/// The total quantity available in inventory for this article.
/// For entity-tracked articles this is a whole number of items; for
/// event-tracked articles it is the net quantity derived from movement events
/// (received minus dispatched) and may be fractional.
/// </param>
public sealed record ArticleInventoryDto(
    double TotalCount
);
