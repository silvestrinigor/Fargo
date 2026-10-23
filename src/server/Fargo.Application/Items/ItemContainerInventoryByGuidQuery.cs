using Fargo.Application.Common;

namespace Fargo.Application.Items;

/// <summary>
/// Queries the article inventory contained in an item container, optionally as
/// it existed at a specific point in time.
/// </summary>
/// <param name="ItemContainerGuid">The container item to inspect.</param>
/// <param name="ArticleGuids">
/// When provided, restricts the inventory to these articles.
/// </param>
/// <param name="IncludeDescendents">
/// When <see langword="true"/>, counts items nested in descendant containers as
/// well, not only items placed directly in the container.
/// </param>
/// <param name="At">
/// The instant, in UTC, at which the inventory is evaluated. When
/// <see langword="null"/>, the current inventory is returned. When provided, the
/// inventory is reconstructed from the item containment history as it was at that
/// instant.
/// </param>
public sealed record ItemContainerInventoryByGuidQuery(
    Guid ItemContainerGuid,
    IReadOnlyCollection<Guid>? ArticleGuids = null,
    bool IncludeDescendents = true,
    DateTimeOffset? At = null
) : IQuery<IReadOnlyCollection<ItemContainerInventoryDto>?>;
