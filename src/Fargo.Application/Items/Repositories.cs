using Fargo.Core.Items;

namespace Fargo.Application.Items;

public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        DateTimeOffset? asOfDateTime = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default
    );
}

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
