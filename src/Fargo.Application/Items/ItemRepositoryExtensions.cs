using Fargo.Domain.Items;

namespace Fargo.Application.Items;

public static class ItemRepositoryExtensions
{
    extension(IItemRepository repository)
    {
        public async Task<Item> GetFoundByGuid(
            Guid itemGuid,
            CancellationToken cancellationToken = default
        )
        {
            var item = await repository.GetByGuid(itemGuid, cancellationToken)
                ?? throw new ItemNotFoundFargoApplicationException(itemGuid);

            return item;
        }
    }
}
