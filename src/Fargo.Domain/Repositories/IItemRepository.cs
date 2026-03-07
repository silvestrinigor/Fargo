using Fargo.Domain.Entities;

namespace Fargo.Domain.Repositories
{
    /// <summary>
    /// Defines the repository contract for managing <see cref="Item"/> entities.
    ///
    /// This repository provides access to persistence operations for items.
    /// </summary>
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