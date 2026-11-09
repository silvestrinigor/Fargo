using Fargo.Application.Operations.SupplyOrders;
using Fargo.Application.Common;
using Fargo.Application.Operations.SupplyOrders.Dtos;
using Fargo.Core.Entities.SupplyOrders;

namespace Fargo.Application.Persistence.Repositories;

/// <summary>
/// Read-side repository for <see cref="SupplyOrder"/> projections.
/// </summary>
public interface ISupplyOrderQueryRepository
{
    /// <summary>
    /// Gets a supply order projection by its identifier, scoped to the partitions
    /// the actor can access.
    /// </summary>
    Task<SupplyOrderDto?> GetInfoByGuidAsync(
        Guid supplyOrderGuid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of supply order projections, scoped to the
    /// partitions the actor can access, optionally filtered by status and a
    /// case-insensitive text search over the reference and description.
    /// </summary>
    Task<PagedResult<SupplyOrderDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        SupplyOrderStatus? status = null,
        Sort? sort = null,
        string? search = null,
        CancellationToken cancellationToken = default);
}
