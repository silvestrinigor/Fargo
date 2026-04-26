using Fargo.Sdk.Events;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionService"/>.</summary>
public sealed class PartitionService : IPartitionService
{
    /// <summary>Initializes a new instance.</summary>
    public PartitionService(IPartitionHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnPartitionUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var partition))
            {
                partition.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnPartitionDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var partition))
            {
                partition.RaiseDeleted();
            }
        });

        this.hub = hub;
    }

    private readonly Dictionary<Guid, Partition> tracked = new();
    private readonly IPartitionHttpClient client;
    private readonly IFargoEventHub hub;

    /// <inheritdoc />
    public async Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(partitionGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return await ToEntityAsync(response.Data!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Partition>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default)
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

    /// <inheritdoc />
    public async Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, parentPartitionGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var partition = new Partition(response.Data, name, description ?? string.Empty, parentPartitionGuid, true, client, MakeDisposeCallback(response.Data));
        tracked[partition.Guid] = partition;
        await hub.InvokeAsync("SubscribeToEntityAsync", partition.Guid);
        return partition;
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(partitionGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<Partition> ToEntityAsync(PartitionResult r)
    {
        var partition = new Partition(r.Guid, r.Name, r.Description, r.ParentPartitionGuid, r.IsActive, client, MakeDisposeCallback(r.Guid), r.EditedByGuid);
        tracked[partition.Guid] = partition;
        await hub.InvokeAsync("SubscribeToEntityAsync", partition.Guid);
        return partition;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
