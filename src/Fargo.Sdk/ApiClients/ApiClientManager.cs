namespace Fargo.Sdk.ApiClients;

/// <summary>Delegate façade that combines all API client responsibilities.</summary>
public sealed class ApiClientManager(IApiClientService service, IApiClientEventSource eventSource) : IApiClientManager
{
    /// <inheritdoc />
    public event EventHandler<ApiClientCreatedEventArgs> Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    /// <inheritdoc />
    public Task<ApiClient> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
        => service.GetAsync(apiClientGuid, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<ApiClient>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default)
        => service.GetManyAsync(page, limit, search, cancellationToken);

    /// <inheritdoc />
    public Task<(ApiClient Client, string PlainKey)> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
        => service.CreateAsync(name, description, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(apiClientGuid, cancellationToken);
}
