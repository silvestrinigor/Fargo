namespace Fargo.Sdk.Partitions;

/// <summary>Provides data for the <see cref="IPartitionManager.Updated"/> event.</summary>
public sealed class PartitionUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated partition.</summary>
    public Guid Guid { get; } = guid;
}
