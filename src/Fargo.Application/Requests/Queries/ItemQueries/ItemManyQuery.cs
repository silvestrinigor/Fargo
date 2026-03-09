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
    /// </param>
    /// <param name="ArticleGuid">
    /// Optional identifier of the associated article used to filter items.
    /// </param>
    /// <param name="TemporalAsOf">
    /// Optional point in time used to retrieve historical data.
    /// When provided, the query returns the state of the items as they existed
    /// at the specified date and time.
    /// </param>
    /// <param name="Pagination">
    /// Pagination parameters used to limit and offset the result set.
    /// </param>
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid = null,
        Guid? ArticleGuid = null,
        DateTimeOffset? TemporalAsOf = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<ItemReadModel>>;

    /// <summary>
    /// Handles the execution of <see cref="ItemManyQuery"/>.
    /// </summary>
    public sealed class ItemManyQueryHandler(
            IItemReadRepository itemRepository
            ) : IQueryHandler<ItemManyQuery, IEnumerable<ItemReadModel>>
    {
        /// <summary>
        /// Executes the query to retrieve multiple items.
        /// </summary>
        /// <param name="query">The query containing filtering and pagination parameters.</param>
        /// <param name="cancellationToken">Token used to cancel the operation.</param>
        /// <returns>
        /// A collection of <see cref="ItemReadModel"/> representing the retrieved items.
        /// </returns>
        public async Task<IEnumerable<ItemReadModel>> Handle(
                ItemManyQuery query,
                CancellationToken cancellationToken = default
                )
            => await itemRepository.GetMany(
                    query.ParentItemGuid,
                    query.ArticleGuid,
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );
    }
}