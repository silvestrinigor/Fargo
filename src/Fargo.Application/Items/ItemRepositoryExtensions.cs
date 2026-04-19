using Fargo.Domain.Items;

namespace Fargo.Application.Items;

/// <summary>
/// Provides extension methods for <see cref="IItemRepository"/>
/// to simplify retrieval operations with validation.
/// </summary>
/// <remarks>
/// These helpers encapsulate common patterns such as retrieving entities
/// and ensuring their existence, promoting consistency and reducing
/// repetitive null-check logic across the application layer.
/// </remarks>
public static class ItemRepositoryExtensions
{
    extension(IItemRepository repository)
    {
        /// <summary>
        /// Retrieves an <see cref="Item"/> by its GUID and ensures it exists.
        /// </summary>
        /// <param name="itemGuid">
        /// The unique identifier of the item.
        /// </param>
        /// <param name="cancellationToken">
        /// A token used to cancel the operation.
        /// </param>
        /// <returns>
        /// The <see cref="Item"/> associated with the specified GUID.
        /// </returns>
        /// <exception cref="ItemNotFoundFargoApplicationException">
        /// Thrown when no item is found with the specified GUID.
        /// </exception>
        /// <remarks>
        /// This method follows a fail-fast approach by throwing an exception
        /// when the requested entity does not exist, eliminating the need
        /// for null checks in the calling code.
        /// </remarks>
        public async Task<Item> GetFoundByGuid(
            Guid itemGuid,
            CancellationToken cancellationToken = default
        )
        {
            var item = await repository.GetByGuid(itemGuid, cancellationToken)
                ?? throw new ItemNotFoundFargoApplicationException(itemGuid);

            return item;
        }
    }
}
