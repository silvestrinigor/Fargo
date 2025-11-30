using Fargo.Application.Interfaces.Solicitations.Queries;
using Fargo.Domain.Interfaces.Repositories;

namespace Fargo.Application.Solicitations.Queries.ItensQueries.GetItemInformation
{
    public class GetItemHandler(IItemRepository itemRepository) : IQueryHandler<GetItemQuery, Task<ItemInformation?>>
    {
        private readonly IItemRepository itemRepository = itemRepository;
        public async Task<ItemInformation?> Handle(GetItemQuery query)
        {
            var item = await itemRepository.GetByGuidAsync(query.ItemGuid);
            
            if (item is null)
            {
                return null;
            }

            return new ItemInformation(
                item.Guid,
                item.Name,
                item.Description,
                item.CreatedAt
            );
        }
    }
}
