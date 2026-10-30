using Fargo.Application.Common;
using Fargo.Core.Barcodes;

namespace Fargo.Application.Articles;

/// <summary>
/// Provides article query operations.
/// </summary>
public interface IArticleQueryRepository
{
    /// <summary>
    /// Retrieves article information by identifier.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
    Task<ArticleDto?> GetInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves article information by barcode.
    /// </summary>
    /// <param name="articleBarcode">
    /// Article barcode information.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Article information if found; otherwise null.
    /// </returns>
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
    /// Retrieves the inventory count of an article contained within the given
    /// containers or any of their descendant containers, resolved in a single
    /// recursive query over the live container tree.
    /// </summary>
    /// <param name="articleGuid">The article to count.</param>
    /// <param name="insideItemContainerGuids">
    /// The root containers whose subtrees are searched. Each container plus every
    /// descendant container is included.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The inventory count of the article across the container subtrees.</returns>
    Task<ArticleInventoryDto> GetInventoryInfoByGuidWithDescendantsAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid> insideItemContainerGuids,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the inventory count of an article as it existed at a specific
    /// instant, reconstructed from the item containment history.
    /// </summary>
    /// <param name="articleGuid">The article to count.</param>
    /// <param name="at">
    /// The instant, in UTC, at which the inventory is evaluated. Each item's
    /// containment is resolved from the history row valid at this instant.
    /// </param>
    /// <param name="insideItemContainerGuids">
    /// When provided, only items that were inside these containers (or, when
    /// descendants are included, their nested containers) at the instant are
    /// counted. When null or empty, every item of the article that existed at the
    /// instant is counted.
    /// </param>
    /// <param name="includeDescendents">
    /// When <see langword="true"/>, items in descendant containers (as nested at
    /// the instant) are included; otherwise only items directly inside the given
    /// containers are counted. Ignored when no containers are provided.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The inventory count of the article at that instant.</returns>
    Task<ArticleInventoryDto> GetInventoryInfoByGuidAtInstantAsync(
        Guid articleGuid,
        DateTimeOffset at,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the event-based inventory of an event-tracked article, computed
    /// as the net of signed movement quantities (received minus dispatched).
    /// </summary>
    /// <remarks>
    /// The net quantity is derived from <c>item_moviment_events</c>. When
    /// containers are provided, only movements into/out of those containers (and,
    /// when descendants are included, their nested containers) are considered;
    /// otherwise every movement of the article is considered, and the result is
    /// the quantity received from outside minus the quantity dispatched to the
    /// outside. When <paramref name="at"/> is provided, only events at or before
    /// that instant are considered.
    /// </remarks>
    /// <param name="articleGuid">The event-tracked article to evaluate.</param>
    /// <param name="insideItemContainerGuids">
    /// When provided, restricts the evaluation to these containers.
    /// </param>
    /// <param name="includeDescendents">
    /// When <see langword="true"/>, includes descendant containers. Ignored when
    /// no containers are provided.
    /// </param>
    /// <param name="at">
    /// The instant, in UTC, at which the inventory is evaluated. When
    /// <see langword="null"/>, the current inventory is returned.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The net quantity of the article.</returns>
    Task<ArticleInventoryDto> GetEventInventoryInfoByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? insideItemContainerGuids = null,
        bool includeDescendents = true,
        DateTimeOffset? at = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves multiple article information records.
    /// </summary>
    /// <param name="pagination">
    /// Pagination configuration.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions.
    /// </param>
    /// <param name="sort">Optional sort specification; when null, results are ordered by name then guid.</param>
    /// <param name="search">Optional case-insensitive text matched against name or description.</param>
    /// <param name="cancellationToken">
    /// Cancellation token.
    /// </param>
    /// <returns>
    /// Collection of article information ordered by guid.
    /// </returns>
    Task<PagedResult<ArticleDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Checks if an article exists by its unique identifier within the specified partitions.
    /// </summary>
    /// <param name="articleGuid">
    /// Article unique identifier to check for existence.
    /// </param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions. If null or empty, no partition filtering is applied.
    /// </param>
    /// <param name="cancellationToken">
    /// Cancellation token to observe while waiting for the task to complete.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains true if the article exists;
    /// otherwise false.
    /// </returns>
    Task<bool> ExistByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Retrieves the tracking mode of an article, or <see langword="null"/> when
    /// the article does not exist within the specified partitions.
    /// </summary>
    /// <param name="articleGuid">The article to inspect.</param>
    /// <param name="childOfAnyOfThesePartitions">
    /// Filters articles inside the provided partitions. When null, no partition
    /// filtering is applied.
    /// </param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>
    /// The article's <see cref="Fargo.Core.Articles.ArticleTrackingMode"/>, or
    /// <see langword="null"/> when the article was not found.
    /// </returns>
    Task<Fargo.Core.Articles.ArticleTrackingMode?> GetTrackingModeByGuidAsync(
        Guid articleGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
