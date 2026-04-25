using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

public static class ApiClientRepositoryExtensions
{
    extension(IApiClientRepository repository)
    {
        public async Task<ApiClient> GetFoundByGuid(Guid guid, CancellationToken cancellationToken = default)
        {
            return await repository.GetByGuid(guid, cancellationToken)
                ?? throw new ApiClientNotFoundFargoApplicationException(guid);
        }
    }
}
