using Fargo.Application.Operations.ServiceAccounts;
using Fargo.Application.Common;
using Fargo.Application.Operations.ServiceAccounts.Dtos;

namespace Fargo.Application.Persistence.Repositories;

/// <summary>
/// Read-side repository for service accounts, projecting to DTOs. Mirrors the
/// command/query repository split used by the other aggregates.
/// </summary>
public interface IServiceAccountQueryRepository
{
    /// <summary>
    /// Gets a single service account projection by id, or null.
    /// </summary>
    /// <param name="guid">The unique identifier of the service account.</param>
    /// <param name="childOfAnyOfThesePartitions">
    /// An optional collection of partition identifiers. When specified, only
    /// service accounts whose partition membership intersects the set are
    /// returned, so the caller only ever sees accounts it has access to. When
    /// <see langword="null"/> no partition filter is applied.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<ServiceAccountDto?> GetInfoByGuidAsync(
        Guid guid,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a page of service account projections.
    /// </summary>
    /// <param name="pagination">The pagination parameters.</param>
    /// <param name="childOfAnyOfThesePartitions">
    /// An optional collection of partition identifiers. When specified, only
    /// service accounts whose partition membership intersects the set are
    /// returned. When <see langword="null"/> no partition filter is applied.
    /// </param>
    /// <param name="cancellationToken">A token used to cancel the operation.</param>
    Task<PagedResult<ServiceAccountDto>> GetManyInfoAsync(
        Pagination pagination,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        CancellationToken cancellationToken = default);
}
