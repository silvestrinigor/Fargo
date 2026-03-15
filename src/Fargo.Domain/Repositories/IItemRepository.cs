using Fargo.Domain.Entities;
using Fargo.Domain.ValueObjects;
using Fargo.Domain.ValueObjects.Entities;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="Item"/> entities.
    /// </summary>
    /// <remarks>
    /// This repository provides persistence access and domain queries related to
    /// <see cref="Item"/> aggregates. It exposes both aggregate retrieval methods
    /// and lightweight projection queries used for read operations.
    /// </remarks>
    public interface IItemRepository
    {
        /// <summary>
        /// Gets an item by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the item.</param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// The matching <see cref="Item"/> if found; otherwise, <see langword="null"/>.
        /// </returns>
        Task<Item?> GetByGuid(
            Guid entityGuid,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets lightweight information about an item by its unique identifier.
        /// </summary>
        /// <param name="entityGuid">The unique identifier of the item.</param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the returned information represents the state of the item
        /// as it existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// An <see cref="ItemInformation"/> projection if the item exists;
        /// otherwise, <see langword="null"/>.
        /// </returns>
        Task<ItemInformation?> GetInfoByGuid(
            Guid entityGuid,
            DateTimeOffset? asOfDateTime = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Gets a paginated collection of item information projections.
        /// </summary>
        /// <param name="pagination">
        /// The pagination configuration used to control the number of returned results
        /// and the starting position of the query.
        /// </param>
        /// <param name="articleGuid">
        /// Optional filter used to retrieve only items associated with a specific article.
        /// When provided, only items belonging to the specified article are returned.
        /// </param>
        /// <param name="asOfDateTime">
        /// Optional point in time used to retrieve historical data.
        /// When provided, the returned results represent the state of the items
        /// as they existed at the specified date and time.
        /// </param>
        /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
        /// <returns>
        /// A read-only collection of <see cref="ItemInformation"/> objects.
        /// </returns>
        Task<IReadOnlyCollection<ItemInformation>> GetManyInfo(
            Pagination pagination,
            Guid? articleGuid = null,
            DateTimeOffset? asOfDateTime = null,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// Adds a new item to the persistence context.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void Add(Item item);

        /// <summary>
        /// Removes an item from the persistence context.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        void Remove(Item item);
    }
}