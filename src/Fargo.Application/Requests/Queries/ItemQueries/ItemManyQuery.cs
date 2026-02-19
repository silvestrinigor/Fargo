using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;
using Fargo.Application.Security;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid = null,
        Guid? ArticleGuid = null,
        DateTime? TemporalAsOf = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<ItemReadModel>>;

    public sealed class ItemManyQueryHandler(
            IItemReadRepository repository,
            ICurrentUser currentUser
            ) : IQueryHandler<ItemManyQuery, IEnumerable<ItemReadModel>>
    {
        public async Task<IEnumerable<ItemReadModel>> Handle(
                ItemManyQuery query,
                CancellationToken cancellationToken = default
                )
            => await repository.GetManyAsync(
                    currentUser.PartitionGuids,
                    query.ParentItemGuid,
                    query.ArticleGuid,
                    query.TemporalAsOf,
                    query.Pagination,
                    cancellationToken
                    );
    }
}