namespace Fargo.Sdk.Partitions;

/// <summary>Delegate façade that implements <see cref="IPartitionManager"/> by composing the focused services.</summary>
public sealed class PartitionManager(IPartitionService service, IPartitionEventSource eventSource) : IPartitionManager
{
    /// <inheritdoc />
    public event EventHandler<PartitionCreatedEventArgs>? Created
    {
        add => eventSource.Created += value;
        remove => eventSource.Created -= value;
    }

    /// <inheritdoc />
    public Task<Partition> GetAsync(Guid partitionGuid, DateTimeOffset? temporalAsOf = null, CancellationToken cancellationToken = default)
        => service.GetAsync(partitionGuid, temporalAsOf, cancellationToken);

    /// <inheritdoc />
    public Task<IReadOnlyCollection<Partition>> GetManyAsync(Guid? parentPartitionGuid = null, DateTimeOffset? temporalAsOf = null, int? page = null, int? limit = null, bool? rootOnly = null, string? search = null, CancellationToken cancellationToken = default)
        => service.GetManyAsync(parentPartitionGuid, temporalAsOf, page, limit, rootOnly, search, cancellationToken);

    /// <inheritdoc />
    public Task<Partition> CreateAsync(string name, string? description = null, Guid? parentPartitionGuid = null, CancellationToken cancellationToken = default)
        => service.CreateAsync(name, description, parentPartitionGuid, cancellationToken);

    /// <inheritdoc />
    public Task DeleteAsync(Guid partitionGuid, CancellationToken cancellationToken = default)
        => service.DeleteAsync(partitionGuid, cancellationToken);
}
