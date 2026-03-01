using Fargo.Domain.Entities;
using Fargo.Domain.Repositories;

namespace Fargo.Domain.Services.ItemServices
{
    public class ItemGetService(
            IItemRepository itemRepository
            )
    {
        public async Task<Item?> GetItem(
                User actor,
                Guid itemGuid,
                CancellationToken cancellationToken = default
                )
        {
            var item = await itemRepository.GetByGuid(
                    itemGuid,
                    [.. actor.PartitionsAccesses.Select(p => p.Guid)],
                    cancellationToken
                    );

            return item;
        }
    }
}