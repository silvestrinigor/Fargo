using Fargo.Api.Partitions;

namespace Fargo.Api.Items;

/// <summary>
/// Provides operations for managing items.
/// </summary>
/// <remarks>
/// An item is a concrete instance of an <see cref="ArticleResult"/>. Items have their own
/// partition scope independent of the article they belong to.
/// </remarks>
public interface IItemClient
{
    /// <summary>
    /// Gets a single item by its unique identifier.
    /// </summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="temporalAsOf">
    /// When provided, returns the state of the item as it existed at this point in time.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the <see cref="ItemResult"/>, or a <see cref="FargoSdkErrorType.NotFound"/>
    /// error if the item does not exist or is not accessible.
    /// </returns>
    Task<FargoSdkResponse<ItemResult>> GetAsync(
        Guid itemGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of items accessible to the current user.
    /// </summary>
    /// <param name="articleGuid">When provided, returns only items associated with the specified article.</param>
    /// <param name="temporalAsOf">When provided, returns historical data as of this point in time.</param>
    /// <param name="page">The zero-based page index.</param>
    /// <param name="limit">The maximum number of results to return.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the collection of items. The collection is empty if none are found.</returns>
    Task<FargoSdkResponse<IReadOnlyCollection<ItemResult>>> GetManyAsync(
        Guid? articleGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new item as an instance of the specified article.
    /// </summary>
    /// <param name="articleGuid">The unique identifier of the article this item is an instance of.</param>
    /// <param name="firstPartition">
    /// The partition to associate with the item on creation.
    /// When <see langword="null"/>, the item is created without a partition
    /// and is publicly accessible to all authenticated users.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>A response containing the <see cref="Guid"/> of the newly created item.</returns>
    Task<FargoSdkResponse<Guid>> CreateAsync(
        Guid articleGuid,
        Guid? firstPartition = null,
        DateTimeOffset? productionDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing item.
    /// </summary>
    /// <param name="itemGuid">The unique identifier of the item to update.</param>
    /// <param name="productionDate">The new production date, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid itemGuid,
        DateTimeOffset? productionDate = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an item.
    /// </summary>
    /// <param name="itemGuid">The unique identifier of the item to delete.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Adds a partition to the specified item.</summary>
    Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid itemGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>Removes a partition from the specified item.</summary>
    Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid itemGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the partitions that directly contain the specified item.
    /// </summary>
    /// <param name="itemGuid">The unique identifier of the item.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A response containing the collection of <see cref="PartitionResult"/> values, or a
    /// <see cref="FargoSdkErrorType.NotFound"/> error if the item does not exist.
    /// The collection is empty if the item exists but the current user has no partition access overlap.
    /// </returns>
    Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid itemGuid,
        CancellationToken cancellationToken = default);
}
