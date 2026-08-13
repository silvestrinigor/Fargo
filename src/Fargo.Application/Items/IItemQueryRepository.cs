using Fargo.Application.Common;

namespace Fargo.Application.Items;

public interface IItemQueryRepository
{
    Task<ItemDto?> GetInfoByGuid(
        Guid entityGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ItemDto>> GetManyInfo(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);
}
