using Fargo.Core.Informations;

namespace Fargo.Core.ServiceAccounts;

/// <summary>
/// Defines the repository contract for querying and persisting
/// <see cref="ServiceAccount"/> entities.
/// </summary>
public interface IServiceAccountRepository
{
    /// <summary>Gets a service account by its unique identifier, or null.</summary>
    Task<ServiceAccount?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    /// <summary>Gets a service account by its client id, or null.</summary>
    Task<ServiceAccount?> GetByClientIdAsync(Nameid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all partitions the active service account has access to: the
    /// partitions granted directly to it plus every descendant of those
    /// partitions in the partition hierarchy (access to a partition implies
    /// access to all of its descendants), mirroring how user partition access is
    /// resolved.
    /// </summary>
    /// <param name="serviceGuid">The unique identifier of the service account.</param>
    /// <param name="cancellationToken">A token used to cancel the asynchronous operation.</param>
    /// <returns>
    /// The identifiers of all partitions the service account can access. Returns
    /// an empty collection when the account is inactive or has no grants.
    /// </returns>
    Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsAsync(
        Guid serviceGuid, CancellationToken cancellationToken = default);

    /// <summary>Adds a new service account to the persistence context.</summary>
    void Add(ServiceAccount serviceAccount);

    /// <summary>Removes a service account from the persistence context.</summary>
    void Remove(ServiceAccount serviceAccount);
}