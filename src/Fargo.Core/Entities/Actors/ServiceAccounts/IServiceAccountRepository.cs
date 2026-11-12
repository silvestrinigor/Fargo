using Fargo.Core.Informations;

namespace Fargo.Core.Entities.Actors.ServiceAccounts;

public interface IServiceAccountRepository
{
    Task<ServiceAccount?> GetByGuidAsync(Guid guid, CancellationToken cancellationToken = default);

    Task<ServiceAccount?> GetByClientIdAsync(Nameid clientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Partitions granted directly plus all their descendants (as for users). Empty when the account
    /// is inactive or has no grants.
    /// </summary>
    Task<IReadOnlyCollection<Guid>> GetAllActivePartitionAccessGuidsAsync(
        Guid serviceGuid, CancellationToken cancellationToken = default);

    void Add(ServiceAccount serviceAccount);

    void Remove(ServiceAccount serviceAccount);
}
