namespace Fargo.Sdk.ApiClients;

/// <summary>Provides CRUD operations for API clients and routes hub events to tracked entities.</summary>
public interface IApiClientService
{
    Task<ApiClient> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<ApiClient>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default);
    Task<(ApiClient Client, string PlainKey)> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default);
}
