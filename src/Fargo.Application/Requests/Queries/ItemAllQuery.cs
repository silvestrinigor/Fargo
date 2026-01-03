using Fargo.Application.Dtos;
using Fargo.Application.Extensions;
using Fargo.Application.Mediators;
using Fargo.Domain.Repositories;

namespace Fargo.Application.Requests.Queries
{
    public sealed record ItemAllQuery() : IQuery<IEnumerable<ItemDto>>;

    public sealed class ItemAllQueryHandler(IItemRepository repository) : IQueryHandlerAsync<ItemAllQuery, IEnumerable<ItemDto>>
    {
        public async Task<IEnumerable<ItemDto>> HandleAsync(ItemAllQuery query, CancellationToken cancellationToken = default)
        {
            var itens = await repository.GetAllAsync(cancellationToken);

            return itens.Select(x => x.ToDto());
        }
    }
}
