using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid = null,
        Guid? ArticleGuid = null,
        DateTime? AtDateTime = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<ItemReadModel>>;

    public sealed class ItemManyQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemReadModel>>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<IEnumerable<ItemReadModel>> HandleAsync(ItemManyQuery query, CancellationToken cancellationToken = default)
            => await repository.GetManyAsync(
                query.ParentItemGuid, 
                query.ArticleGuid, 
                query.AtDateTime, 
                query.Pagination,
                cancellationToken);
    }
}
