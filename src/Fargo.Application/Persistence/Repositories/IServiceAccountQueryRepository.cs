using Fargo.Application.Common.Paging;
using Fargo.Application.Operations.ServiceAccounts.Dtos;

namespace Fargo.Application.Persistence.Repositories;

/// <remarks>
/// For every <c>childOfAnyOfThesePartitions</c> parameter, null applies no partition filter; otherwise
/// only service accounts whose partitions intersect the set are returned.
/// </remarks>
public interface IServiceAccountQueryRepository
{
    Task<ServiceAccountDto?> GetInfoByGuidAsync(
        Guid guid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);
}
