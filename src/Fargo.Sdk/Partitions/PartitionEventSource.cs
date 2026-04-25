using Fargo.Sdk.Events;

namespace Fargo.Sdk.Partitions;

/// <summary>Default implementation of <see cref="IPartitionEventSource"/>.</summary>
public sealed class PartitionEventSource : IPartitionEventSource
{
    public PartitionEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnPartitionCreated", guid =>
            Created?.Invoke(this, new PartitionCreatedEventArgs(guid)));
    }

    public event EventHandler<PartitionCreatedEventArgs>? Created;
}
