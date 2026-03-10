using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    /// <summary>
    /// Query used to retrieve multiple items.
    /// </summary>
    /// <param name="ParentItemGuid">
    /// Optional identifier of the parent item used to filter hierarchical item relationships.
    /// When provided, only items that belong to the specified parent item are returned.
    /// </param>
    /// <param name="ArticleGuid">
    /// Optional identifier of the associated article used to filter items.
    /// When provided, only items associated with the specified article are returned.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the items
    /// as they existed at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Optional pagination parameters used to limit and offset the result set.
    /// If not provided, a default pagination configuration is used.
    /// </param>
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid = null,
        Guid? ArticleGuid = null,
        DateTimeOffset? TemporalAsOf = null,
        Pagination? Pagination = null
    ) : IQuery<IReadOnlyCollection<ItemReadModel>>;

    /// <summary>
    /// Handles the execution of <see cref="ItemManyQuery"/>.
    /// </summary>
    public sealed class ItemManyQueryHandler(
            IItemReadRepository itemRepository
            ) : IQueryHandler<ItemManyQuery, IReadOnlyCollection<ItemReadModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple items.
        /// </summary>
        /// <param name="query">
        /// The query containing filtering, temporal reference,
        /// and optional pagination parameters.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to cancel the operation.
        /// </param>
        /// <returns>
        /// A read-only collection of <see cref="ItemReadModel"/> representing
        /// the items that match the specified filters and pagination.
        /// </returns>
        /// <remarks>
        /// If pagination is not provided, the query uses
        /// <see cref="Pagination.First20Pages"/> as the default.
        /// </remarks>
        public async Task<IReadOnlyCollection<ItemReadModel>> Handle(
                ItemManyQuery query,
                CancellationToken cancellationToken = default
                )
        {
            var items = await itemRepository.GetMany(
                    query.Pagination ?? Pagination.First20Pages,
                    query.ParentItemGuid,
                    query.ArticleGuid,
                    query.TemporalAsOf,
                    cancellationToken
                    );

            return items;
        }
    }
}