using Fargo.Api.Events;
using System.Collections.Concurrent;

namespace Fargo.Api.ApiClients;

/// <summary>Default implementation of <see cref="IApiClientService"/>.</summary>
public sealed class ApiClientService : IApiClientService
{
    /// <summary>Initializes a new instance, subscribing to hub events for updates and deletions.</summary>
    /// <param name="client">The HTTP client for API client endpoints.</param>
    /// <param name="hub">The event hub used to receive real-time notifications.</param>
    public ApiClientService(IApiClientHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnApiClientUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var apiClient))
            {
                apiClient.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnApiClientDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var apiClient))
            {
                apiClient.RaiseDeleted();
            }
        });
    }

    private readonly ConcurrentDictionary<Guid, ApiClient> tracked = new();
    private readonly IApiClientHttpClient client;

    /// <inheritdoc />
    public async Task<ApiClient> GetAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(apiClientGuid, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return ToEntity(response.Data!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<ApiClient>> GetManyAsync(int? page = null, int? limit = null, string? search = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(page, limit, search, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data!.Select(ToEntity).ToList();
    }

    /// <inheritdoc />
    public async Task<(ApiClient Client, string PlainKey)> CreateAsync(string name, string? description = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var r = response.Data!;
        var entity = new ApiClient(r.Guid, name, description ?? string.Empty, true, client, () =>
        {
            tracked.TryRemove(r.Guid, out _);
            return ValueTask.CompletedTask;
        });
        return (tracked.GetOrAdd(r.Guid, entity), r.PlainKey);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid apiClientGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(apiClientGuid, cancellationToken);
        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        tracked.TryRemove(apiClientGuid, out _);
    }

    private ApiClient ToEntity(ApiClientResult r)
    {
        if (tracked.TryGetValue(r.Guid, out var existing))
        {
            return existing;
        }

        var entity = new ApiClient(r.Guid, r.Name, r.Description, r.IsActive, client, () =>
        {
            tracked.TryRemove(r.Guid, out _);
            return ValueTask.CompletedTask;
        }, r.EditedByGuid);

        return tracked.GetOrAdd(r.Guid, entity);
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error);
}
