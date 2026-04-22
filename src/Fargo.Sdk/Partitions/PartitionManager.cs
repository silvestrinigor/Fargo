using Fargo.Sdk.Events;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionManager"/>.</summary>
public sealed class PartitionManager : IPartitionManager
{
    internal PartitionManager(IPartitionClient client, FargoHubConnection hub)
    {
        this.client = client;
        this.hub = hub;

        hub.On<Guid>("OnPartitionCreated", guid =>
            Created?.Invoke(this, new PartitionCreatedEventArgs(guid)));

        hub.On<Guid>("OnPartitionUpdated", guid =>
        {
            if (_tracked.TryGetValue(guid, out var partition))
            {
                partition.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnPartitionDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var partition))
            {
                partition.RaiseDeleted();
            }
        });
    }

    public event EventHandler<PartitionCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, Partition> _tracked = new();
    private readonly IPartitionClient client;
    private readonly FargoHubConnection hub;

    public async Task<Partition> GetAsync(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(partitionGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    public async Task<IReadOnlyCollection<Partition>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        bool? rootOnly = null,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(parentPartitionGuid, temporalAsOf, page, limit, rootOnly, search, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var entities = new List<Partition>();
        foreach (var r in response.Data!)
        {
            entities.Add(await ToEntityAsync(r));
        }

        return entities;
    }

    public async Task<Partition> CreateAsync(
        string name,
        string? description = null,
        Guid? parentPartitionGuid = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, parentPartitionGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var partition = new Partition(
            response.Data,
            name,
            description ?? string.Empty,
            parentPartitionGuid,
            true,
            client,
            MakeDisposeCallback(response.Data));
        _tracked[partition.Guid] = partition;
        await hub.InvokeAsync("SubscribeToEntityAsync", partition.Guid);
        return partition;
    }

    public async Task DeleteAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(partitionGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<Partition> ToEntityAsync(PartitionResult r)
    {
        var partition = new Partition(
            r.Guid,
            r.Name,
            r.Description,
            r.ParentPartitionGuid,
            r.IsActive,
            client,
            MakeDisposeCallback(r.Guid),
            r.EditedByGuid);
        _tracked[partition.Guid] = partition;
        await hub.InvokeAsync("SubscribeToEntityAsync", partition.Guid);
        return partition;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        _tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
