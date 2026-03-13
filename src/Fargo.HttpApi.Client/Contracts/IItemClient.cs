using Fargo.Application.Common;
using Fargo.Application.Models.ItemModels;

namespace Fargo.HttpApi.Client.Interfaces
{
    /// <summary>
    /// Defines the contract for item-related HTTP API operations.
    /// </summary>
    public interface IItemClient
    {
        /// <summary>
        /// Gets a single item by its identifier.
        /// </summary>
        Task<ItemReadModel?> GetSingleAsync(
            Guid itemGuid,
            DateTimeOffset? temporalAsOf = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets multiple items with optional filtering, pagination and temporal query.
        /// </summary>
        Task<IReadOnlyCollection<ItemReadModel>> GetManyAsync(
            Guid? articleGuid = null,
            DateTimeOffset? temporalAsOf = null,
            Page? page = null,
            Limit? limit = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new item and returns its identifier.
        /// </summary>
        Task<Guid> CreateAsync(
            ItemCreateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates an existing item.
        /// </summary>
        Task UpdateAsync(
            Guid itemGuid,
            ItemUpdateModel model,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an item.
        /// </summary>
        Task DeleteAsync(
            Guid itemGuid,
            CancellationToken cancellationToken = default);
    }
}