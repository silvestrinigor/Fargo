using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Items;

/// <summary>
/// Represents a live item entity — a concrete instance of an <see cref="ArticleResult"/>.
/// </summary>
public sealed class Item
{
    internal Item(Guid guid, Guid articleGuid, IItemClient client)
    {
        Guid = guid;
        ArticleGuid = articleGuid;
        this.client = client;
    }

    private readonly IItemClient client;

    /// <summary>The unique identifier of the item.</summary>
    public Guid Guid { get; }

    /// <summary>The unique identifier of the article this item is an instance of.</summary>
    public Guid ArticleGuid { get; }

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
}
