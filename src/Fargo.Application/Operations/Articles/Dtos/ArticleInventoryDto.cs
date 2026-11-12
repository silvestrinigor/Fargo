namespace Fargo.Application.Operations.Articles.Dtos;

/// <param name="TotalCount">
/// Whole number for entity-tracked articles; for event-tracked articles the net of movement
/// events (received minus dispatched), which may be fractional.
/// </param>
public sealed record ArticleInventoryDto(
    double TotalCount
);
