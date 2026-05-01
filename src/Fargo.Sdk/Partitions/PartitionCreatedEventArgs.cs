namespace Fargo.Api.Partitions;

/// <summary>Provides data for the <see cref="IPartitionEventSource.Created"/> event.</summary>
public sealed class PartitionCreatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created partition.</summary>
    public Guid Guid { get; } = guid;
}
