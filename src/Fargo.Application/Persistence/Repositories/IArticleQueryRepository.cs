using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.Articles;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Persistence.Repositories;

/// <remarks>
/// For every <c>childOfAnyOfThesePartitions</c> parameter, null applies no partition filter and an
/// empty collection matches nothing; an article matches when it belongs to at least one of the
/// partitions, so an article with no partitions never matches a filter.
/// </remarks>
public interface IArticleQueryRepository
{
    Task<ArticleDto?> GetInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleDto?> GetInfoByBarcodeAsync(
        Barcode articleBarcode,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<ArticleInventoryDto> GetInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the article in the given containers and all their descendant containers, resolved in a
    /// single recursive query over the live container tree.
    /// </summary>
    Task<ArticleInventoryDto> GetInventoryInfoByGuidWithDescendantsAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid> insideItemContainerGuids,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Counts the article as it existed at <paramref name="at"/>, reconstructed from the item
    /// containment history.
    /// </summary>
    /// <param name="insideItemContainerGuids">
    /// Null or empty counts every item of the article that existed at the instant. Otherwise only items
    /// inside these containers at the instant are counted.
    /// </param>
    /// <param name="includeDescendents">
    /// Also count items in descendant containers (as nested at the instant). Ignored when no containers
    /// are given.
    /// </param>
    Task<ArticleInventoryDto> GetInventoryInfoByGuidAtInstantAsync(
        Guid articleGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Net quantity of an event-tracked article: received minus dispatched, from the signed movement
    /// quantities in <c>item_moviment_events</c>.
    /// </summary>
    /// <remarks>
    /// With containers, only movements into or out of them (and their descendants when
    /// <paramref name="includeDescendents"/> is set, which is otherwise ignored) count. Without
    /// containers, every movement counts: received from outside minus dispatched to the outside.
    /// A null <paramref name="at"/> means now; otherwise only events at or before that instant count.
    /// </remarks>
    Task<ArticleInventoryDto> GetEventInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        DateTimeOffset? at = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Results are ordered by <paramref name="sort"/>, or by name then guid when it is null.
    /// </summary>
    /// <param name="search">Case-insensitive text matched against name or description.</param>
    Task<PagedResult<ArticleDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Null when the article does not exist within the specified partitions.
    /// </summary>
    Task<Core.Entities.Articles.ArticleTrackingMode?> GetTrackingModeByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
