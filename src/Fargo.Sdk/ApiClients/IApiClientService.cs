namespace Fargo.Api.ApiClients;

/// <summary>Provides CRUD operations for API clients and routes hub events to tracked entities.</summary>
public interface IApiClientService
{
    /// <summary>Retrieves a single API client by its unique identifier.</summary>
    /// <param name="apiClientGuid">The unique identifier of the API client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the API client does not exist or is not accessible.</exception>
    Task<ApiClient> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged collection of API clients.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">The maximum number of results per page.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<IReadOnlyCollection<ApiClient>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new API client and returns the entity together with the plain-text API key.</summary>
    /// <param name="name">The display name for the new API client.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<(ApiClient Client, string PlainKey)> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);

    /// <summary>Deletes an API client by its unique identifier.</summary>
    /// <param name="apiClientGuid">The unique identifier of the API client to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
}
