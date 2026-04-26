using Fargo.Sdk.Events;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionEventSource"/>.</summary>
public sealed class PartitionEventSource : IPartitionEventSource
{
    /// <summary>Initializes a new instance.</summary>
    public PartitionEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnPartitionCreated", guid =>
            Created?.Invoke(this, new PartitionCreatedEventArgs(guid)));
    }

    /// <inheritdoc />
    public event EventHandler<PartitionCreatedEventArgs>? Created;
}
