namespace Fargo.Application.Events;

/// <summary>
/// Publishes domain events after entity mutations are persisted.
/// </summary>
/// <remarks>
/// This interface is defined in the Application layer and implemented by the API layer
/// using SignalR, keeping the Application layer free of infrastructure dependencies.
/// </remarks>
public interface IFargoEventPublisher
{
    Task PublishArticleCreated(Guid guid, CancellationToken ct = default);
    Task PublishArticleUpdated(Guid guid, CancellationToken ct = default);
    Task PublishArticleDeleted(Guid guid, CancellationToken ct = default);

    Task PublishItemCreated(Guid guid, Guid articleGuid, CancellationToken ct = default);
    Task PublishItemUpdated(Guid guid, CancellationToken ct = default);
    Task PublishItemDeleted(Guid guid, CancellationToken ct = default);

    Task PublishUserCreated(Guid guid, string nameid, CancellationToken ct = default);
    Task PublishUserUpdated(Guid guid, CancellationToken ct = default);
    Task PublishUserDeleted(Guid guid, CancellationToken ct = default);

    Task PublishUserGroupCreated(Guid guid, string nameid, CancellationToken ct = default);
    Task PublishUserGroupUpdated(Guid guid, CancellationToken ct = default);
    Task PublishUserGroupDeleted(Guid guid, CancellationToken ct = default);

    Task PublishPartitionCreated(Guid guid, CancellationToken ct = default);
    Task PublishPartitionUpdated(Guid guid, CancellationToken ct = default);
    Task PublishPartitionDeleted(Guid guid, CancellationToken ct = default);
}
