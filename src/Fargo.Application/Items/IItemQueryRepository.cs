using Fargo.Application.Common;

namespace Fargo.Application.Items;

public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuidAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemDto>> GetLocationInfoByGuidOrderByDepthAsync(
        Guid itemGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);
}
