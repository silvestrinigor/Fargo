using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Items;

/// <summary>
/// Represents a live item entity — a concrete instance of an <see cref="ArticleResult"/>.
/// Dispose to unsubscribe from real-time events.
/// </summary>
public sealed class Item : IAsyncDisposable
{
    internal Item(Guid guid, Guid articleGuid, IItemHttpClient client, Func<ValueTask>? onDispose = null, Guid? editedByGuid = null)
    {
        Guid = guid;
        ArticleGuid = articleGuid;
        EditedByGuid = editedByGuid;
        this.client = client;
        _onDispose = onDispose;
    }

    private readonly IItemHttpClient client;
    private readonly Func<ValueTask>? _onDispose;

    /// <summary>The unique identifier of the item.</summary>
    public Guid Guid { get; }

    /// <summary>The unique identifier of the article this item is an instance of.</summary>
    public Guid ArticleGuid { get; }

    /// <summary>The GUID of the user who last edited this item, or <see langword="null"/> if never edited.</summary>
    public Guid? EditedByGuid { get; }

    /// <summary>Raised when this item is updated by any authenticated client.</summary>
    public event EventHandler<ItemUpdatedEventArgs>? Updated;

    /// <summary>Raised when this item is deleted by any authenticated client.</summary>
    public event EventHandler<ItemDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ItemUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new ItemDeletedEventArgs(Guid));

    /// <summary>Gets the partitions that directly contain this item.</summary>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    /// <summary>Adds a partition to this item.</summary>
    public Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
        => client.AddPartitionAsync(Guid, partitionGuid, cancellationToken);

    /// <summary>Removes a partition from this item.</summary>
    public Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
        => client.RemovePartitionAsync(Guid, partitionGuid, cancellationToken);

    /// <inheritdoc/>
    public ValueTask DisposeAsync() => _onDispose?.Invoke() ?? ValueTask.CompletedTask;
}
