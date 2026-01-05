using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ItemManyQuery(
        Guid? ArticleGuid
        ) : IQuery<IEnumerable<ItemDto>>;

    public sealed class ItemManyQueryHandler(IItemReadRepository repository) : IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>>
    {
        private readonly IItemReadRepository repository = repository;

        public async Task<IEnumerable<ItemDto>> HandleAsync(ItemManyQuery query, CancellationToken cancellationToken = default)
        {
            var itens = await repository.GetManyAsync(query.ArticleGuid, cancellationToken);

            return itens.Select(x => x.ToDto());
        }
    }
}
