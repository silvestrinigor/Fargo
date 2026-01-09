using Fargo.Application.Dtos;
using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries.ItemQueries
{
    public sealed record ItemManyQuery(
        Guid? ParentItemGuid,
        Guid? ArticleGuid,
        DateTime? AtDateTime,
        PaginationDto Pagination
        ) : IQuery<IEnumerable<ItemDto>>;

    public sealed class ItemManyQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<IEnumerable<ItemDto>> HandleAsync(ItemManyQuery query, CancellationToken cancellationToken = default)
        {
            var itens = await repository.GetManyAsync(query.ParentItemGuid, query.ArticleGuid, query.AtDateTime, query.Pagination.Skip, query.Pagination.Limit, cancellationToken);

            return itens.Select(x => x.ToDto());
        }
    }
}
