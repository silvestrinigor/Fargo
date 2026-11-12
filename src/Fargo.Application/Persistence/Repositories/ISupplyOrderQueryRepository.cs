using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos.SupplyOrders;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Persistence.Repositories;

public interface ISupplyOrderQueryRepository
{
    Task<SupplyOrderDto?> GetInfoByGuidAsync(
        Guid supplyOrderGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// <paramref name="search"/> is a case-insensitive match over the reference and description.
    /// </summary>
    Task<PagedResult<SupplyOrderDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        SupplyOrderStatus? status = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
