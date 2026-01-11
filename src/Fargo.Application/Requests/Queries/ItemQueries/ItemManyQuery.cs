using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories.ItemRepositories;
using Fargo.Domain.ValueObjects;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid = null,
        Guid? ArticleGuid = null,
        DateTime? AtDateTime = null,
        Pagination Pagination = default
        ) : IQuery<IEnumerable<ItemDto>>;

    public sealed class ItemManyQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<IEnumerable<ItemDto>> HandleAsync(ItemManyQuery query, CancellationToken cancellationToken = default)
        {
            var itens = await repository.GetManyAsync(
                query.ParentItemGuid, 
                query.ArticleGuid, 
                query.AtDateTime, 
                query.Pagination,
                cancellationToken);

            return itens.Select(x => x.ToDto());
        }
    }
}
