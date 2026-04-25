using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

public interface IApiClientRepository
{
    void Add(ApiClient client);
    void Remove(ApiClient client);
    Task<ApiClient?> GetByGuid(Guid guid, CancellationToken cancellationToken = default);
    Task<bool> ExistsByGuid(Guid guid, CancellationToken cancellationToken = default);
}
