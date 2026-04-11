namespace Fargo.Application.Events;

/// <summary>
/// Publishes domain events after entity mutations are persisted.
/// Each method receives the partition guids the affected entity belongs to so that
/// the implementation can route events only to clients with matching access.
/// </summary>
/// <remarks>
/// This interface is defined in the Application layer and implemented by the API layer
/// using SignalR, keeping the Application layer free of infrastructure dependencies.
/// </remarks>
public interface IFargoEventPublisher
{
    Task PublishArticleCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishArticleUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishArticleDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    Task PublishItemCreated(Guid guid, Guid articleGuid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishItemUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishItemDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    Task PublishUserCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishUserUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishUserDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    Task PublishUserGroupCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishUserGroupUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishUserGroupDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);

    Task PublishPartitionCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishPartitionUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
    Task PublishPartitionDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default);
}
