namespace Fargo.Sdk.Partitions;

/// <summary>
/// Combined partition interface: CRUD and Created events.
/// Inject this when you need everything, or inject the narrower interfaces individually.
/// </summary>
public interface IPartitionManager : IPartitionService, IPartitionEventSource { }
