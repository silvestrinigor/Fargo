using Fargo.Sdk.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionManager"/>.</summary>
public sealed class PartitionManager : IPartitionManager
{
    internal PartitionManager(IPartitionClient client, FargoHubConnection hub, ILogger logger)
    {
        this.client = client;
        this.logger = logger;

        hub.On<Guid>("OnPartitionCreated", guid =>
            Created?.Invoke(this, new PartitionCreatedEventArgs(guid)));

        hub.On<Guid>("OnPartitionUpdated", guid =>
            Updated?.Invoke(this, new PartitionUpdatedEventArgs(guid)));

        hub.On<Guid>("OnPartitionDeleted", guid =>
            Deleted?.Invoke(this, new PartitionDeletedEventArgs(guid)));
    }

    public event EventHandler<PartitionCreatedEventArgs>? Created;
    public event EventHandler<PartitionUpdatedEventArgs>? Updated;
    public event EventHandler<PartitionDeletedEventArgs>? Deleted;

    private readonly IPartitionClient client;
    private readonly ILogger logger;

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

        return new Partition(
            response.Data,
            name,
            description ?? string.Empty,
            parentPartitionGuid,
            true,
            client,
            logger);
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

    private Partition ToEntity(PartitionResult r) => new(
        r.Guid,
        r.Name,
        r.Description,
        r.ParentPartitionGuid,
        r.IsActive,
        client,
        logger);

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
