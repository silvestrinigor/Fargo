namespace Fargo.Sdk.Partitions;

/// <summary>Provides data for the <see cref="IPartitionManager.Deleted"/> event.</summary>
public sealed class PartitionDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted partition.</summary>
    public Guid Guid { get; } = guid;
}
