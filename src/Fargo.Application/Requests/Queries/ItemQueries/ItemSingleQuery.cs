using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    /// <summary>
    /// Query used to retrieve a single item by its unique identifier.
    /// </summary>
    /// <param name="ItemGuid">
    /// The unique identifier of the item.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the item
    /// as it existed at the specified date and time.
    /// </param>
    public sealed record ItemSingleQuery(
        Guid ItemGuid,
        DateTimeOffset? TemporalAsOf = null
        ) : IQuery<ItemReadModel?>;

    /// <summary>
    /// Handles the execution of <see cref="ItemSingleQuery"/>.
    /// </summary>
    public sealed class ItemSingleQueryHandler(
            IItemQueries itemRepository
            ) : IQueryHandler<ItemSingleQuery, ItemReadModel?>
    {
        /// <summary>
        /// Executes the query to retrieve a single item.
        /// </summary>
        /// <param name="query">The query containing the item identifier.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// The <see cref="ItemReadModel"/> if the item exists; otherwise <c>null</c>.
        /// </returns>
        public async Task<ItemReadModel?> Handle(
                ItemSingleQuery query,
                CancellationToken cancellationToken = default
                )
            => await itemRepository.GetByGuid(
                query.ItemGuid,
                query.TemporalAsOf,
                cancellationToken
                );
    }
}