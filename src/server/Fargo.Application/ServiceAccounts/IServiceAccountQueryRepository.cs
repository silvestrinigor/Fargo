using Fargo.Application.Common;

namespace Fargo.Application.ServiceAccounts;

/// <summary>
/// Read-side repository for service accounts, projecting to DTOs. Mirrors the
/// command/query repository split used by the other aggregates.
/// </summary>
public interface IServiceAccountQueryRepository
{
    /// <summary>Gets a single service account projection by id, or null.</summary>
    Task<ServiceAccountDto?> GetInfoByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Gets a page of service account projections.</summary>
    Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        CancellationToken cancellationToken = default);
}
