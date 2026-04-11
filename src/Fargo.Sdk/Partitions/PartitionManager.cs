using Fargo.Sdk.Events;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionManager"/>.</summary>
public sealed class PartitionManager : IPartitionManager
{
    internal PartitionManager(IPartitionClient client, FargoHubConnection hub)
    {
        this.client = client;

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

        return ToEntity(response.Data!);
    }

    public async Task<IReadOnlyCollection<Partition>> GetManyAsync(
        Guid? parentPartitionGuid = null,
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(parentPartitionGuid, temporalAsOf, page, limit, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data!.Select(ToEntity).ToList();
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
            client);
        _tracked[partition.Guid] = partition;
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

    private Partition ToEntity(PartitionResult r)
    {
        var partition = new Partition(
            r.Guid,
            r.Name,
            r.Description,
            r.ParentPartitionGuid,
            r.IsActive,
            client);
        _tracked[partition.Guid] = partition;
        return partition;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
