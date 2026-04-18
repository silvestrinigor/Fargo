namespace Fargo.Domain.Partitions;

/// <summary>
/// Marker interface for domain entities that belong to one or more partitions.
/// </summary>
/// <remarks>
/// Implementing this interface signals that an entity participates in the
/// partition-based access control (PBAC) model, meaning its visibility and
/// mutability are governed by the actor's partition access set.
/// </remarks>
public interface IPartitionEntity : IEntity;
