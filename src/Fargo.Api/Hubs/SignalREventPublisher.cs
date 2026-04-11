using Fargo.Application.Events;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

/// <summary>
/// Implements <see cref="IFargoEventPublisher"/> using SignalR.
/// Created events are broadcast to partition groups so clients discover new entities.
/// Updated/Deleted events are broadcast to entity-specific groups (<c>e:{guid}</c>) so
/// only clients that fetched the entity receive them. Admin clients always receive all
/// events via the <c>"fargo-admin"</c> group.
/// </summary>
public sealed class SignalREventPublisher(IHubContext<FargoEventHub, IFargoEventClient> hub) : IFargoEventPublisher
{
    public Task PublishArticleCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => PartitionClients(partitionGuids).OnArticleCreated(guid);

    public Task PublishArticleUpdated(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnArticleUpdated(guid);

    public Task PublishArticleDeleted(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnArticleDeleted(guid);

    public Task PublishItemCreated(Guid guid, Guid articleGuid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => PartitionClients(partitionGuids).OnItemCreated(guid, articleGuid);

    public Task PublishItemUpdated(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnItemUpdated(guid);

    public Task PublishItemDeleted(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnItemDeleted(guid);

    public Task PublishUserCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => PartitionClients(partitionGuids).OnUserCreated(guid, nameid);

    public Task PublishUserUpdated(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnUserUpdated(guid);

    public Task PublishUserDeleted(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnUserDeleted(guid);

    public Task PublishUserGroupCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => PartitionClients(partitionGuids).OnUserGroupCreated(guid, nameid);

    public Task PublishUserGroupUpdated(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnUserGroupUpdated(guid);

    public Task PublishUserGroupDeleted(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnUserGroupDeleted(guid);

    public Task PublishPartitionCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => PartitionClients(partitionGuids).OnPartitionCreated(guid);

    public Task PublishPartitionUpdated(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnPartitionUpdated(guid);

    public Task PublishPartitionDeleted(Guid guid, CancellationToken ct = default)
        => EntityClients(guid).OnPartitionDeleted(guid);

    private IFargoEventClient PartitionClients(IReadOnlyCollection<Guid> partitionGuids)
    {
        var groups = partitionGuids.Select(g => g.ToString()).Append("fargo-admin").ToList();
        return hub.Clients.Groups(groups);
    }

    private IFargoEventClient EntityClients(Guid entityGuid)
        => hub.Clients.Groups([$"e:{entityGuid}", "fargo-admin"]);
}
