namespace Fargo.Sdk.ApiClients;

/// <summary>Low-level HTTP transport for API client endpoints.</summary>
public interface IApiClientHttpClient
{
    /// <summary>Retrieves a single API client by its unique identifier.</summary>
    /// <param name="apiClientGuid">The unique identifier of the API client.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ApiClientResult>> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);

    /// <summary>Retrieves a paged collection of API clients.</summary>
    /// <param name="page">The one-based page number.</param>
    /// <param name="limit">The maximum number of results per page.</param>
    /// <param name="search">An optional search term to filter by name.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<IReadOnlyCollection<ApiClientResult>>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default);

    /// <summary>Creates a new API client and returns the generated plain-text key.</summary>
    /// <param name="name">The display name for the new API client.</param>
    /// <param name="description">An optional description.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<ApiClientCreatedResult>> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);

    /// <summary>Updates the properties of an existing API client.</summary>
    /// <param name="apiClientGuid">The unique identifier of the API client to update.</param>
    /// <param name="name">The new name, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="description">The new description, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="isActive">The new active state, or <see langword="null"/> to leave unchanged.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> UpdateAsync(Guid apiClientGuid, string? name, string? description, bool? isActive, CancellationToken cancellationToken = default);

    /// <summary>Deletes an API client by its unique identifier.</summary>
    /// <param name="apiClientGuid">The unique identifier of the API client to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    Task<FargoSdkResponse<EmptyResult>> DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
}
