using Fargo.Application.Common;

namespace Fargo.Application.Items;

public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidOrderedByDepthAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemMovimentDto>?> GetItemMovimentsInfoByGuidOrderedByOccurredAtAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemDto>> GetManyInfoOrderedByGuidAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );

    Task<IReadOnlyCollection<ItemContainerInventoryDto>> GetInventoryInfoByGuidAsync(
        IReadOnlyCollection<Guid>? itemContainerGuids = null,
        IReadOnlyCollection<Guid>? articleGuids = null,
        CancellationToken cancellationToken = default
    );

    Task<bool> ExistByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default
    );
}
