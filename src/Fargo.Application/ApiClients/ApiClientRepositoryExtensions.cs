using Fargo.Domain.ApiClients;

namespace Fargo.Application.ApiClients;

/// <summary>Extension methods for <see cref="IApiClientRepository"/>.</summary>
public static class ApiClientRepositoryExtensions
{
    extension(IApiClientRepository repository)
    {
        /// <summary>Returns the API client with the given identifier, or throws <see cref="ApiClientNotFoundFargoApplicationException"/> if not found.</summary>
        /// <param name="guid">The unique identifier to look up.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <exception cref="ApiClientNotFoundFargoApplicationException">Thrown when no API client with the given identifier exists.</exception>
        public async Task<ApiClient> GetFoundByGuid(Guid guid, CancellationToken cancellationToken = default)
        {
            return await repository.GetByGuid(guid, cancellationToken)
                ?? throw new ApiClientNotFoundFargoApplicationException(guid);
        }
    }
}
