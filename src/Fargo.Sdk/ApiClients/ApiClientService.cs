using Fargo.Sdk.Events;

namespace Fargo.Sdk.ApiClients;

/// <summary>Default implementation of <see cref="IApiClientService"/>.</summary>
public sealed class ApiClientService : IApiClientService
{
    public ApiClientService(IApiClientHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnApiClientUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var apiClient))
                apiClient.RaiseUpdated();
        });

        hub.On<Guid>("OnApiClientDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var apiClient))
                apiClient.RaiseDeleted();
        });
    }

    private readonly Dictionary<Guid, ApiClient> tracked = new();
    private readonly IApiClientHttpClient client;

    public async Task<ApiClient> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(apiClientGuid, cancellationToken);
        if (!response.IsSuccess) ThrowError(response.Error!);
        return ToEntity(response.Data!);
    }

    public async Task<IReadOnlyCollection<ApiClient>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(page, limit, search, cancellationToken);
        if (!response.IsSuccess) ThrowError(response.Error!);
        return response.Data!.Select(ToEntity).ToList();
    }

    public async Task<(ApiClient Client, string PlainKey)> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, cancellationToken);
        if (!response.IsSuccess) ThrowError(response.Error!);
        var r = response.Data!;
        var entity = new ApiClient(r.Guid, name, description ?? string.Empty, true, client, () =>
        {
            tracked.Remove(r.Guid);
            return ValueTask.CompletedTask;
        });
        tracked[r.Guid] = entity;
        return (entity, r.PlainKey);
    }

    public async Task DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(apiClientGuid, cancellationToken);
        if (!response.IsSuccess) ThrowError(response.Error!);
        tracked.Remove(apiClientGuid);
    }

    private ApiClient ToEntity(ApiClientResult r)
    {
        if (tracked.TryGetValue(r.Guid, out var existing))
            return existing;

        var entity = new ApiClient(r.Guid, r.Name, r.Description, r.IsActive, client, () =>
        {
            tracked.Remove(r.Guid);
            return ValueTask.CompletedTask;
        }, r.EditedByGuid);

        tracked[r.Guid] = entity;
        return entity;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
