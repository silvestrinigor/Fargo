namespace Fargo.Api.Partitions;

/// <summary>Broadcasts the hub <c>OnPartitionCreated</c> event as a typed .NET event.</summary>
public interface IPartitionEventSource
{
    /// <summary>Raised when any authenticated client creates a partition.</summary>
    event EventHandler<PartitionCreatedEventArgs>? Created;
}
