using Fargo.Application.Events;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

/// <summary>
/// Implements <see cref="IFargoEventPublisher"/> by broadcasting events to partition-specific
/// SignalR groups via <see cref="FargoEventHub"/>. Admin clients receive all events via
/// the <c>"fargo-admin"</c> group.
/// </summary>
public sealed class SignalREventPublisher(IHubContext<FargoEventHub, IFargoEventClient> hub) : IFargoEventPublisher
{
    public Task PublishArticleCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnArticleCreated(guid);

    public Task PublishArticleUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnArticleUpdated(guid);

    public Task PublishArticleDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnArticleDeleted(guid);

    public Task PublishItemCreated(Guid guid, Guid articleGuid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnItemCreated(guid, articleGuid);

    public Task PublishItemUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnItemUpdated(guid);

    public Task PublishItemDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnItemDeleted(guid);

    public Task PublishUserCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserCreated(guid, nameid);

    public Task PublishUserUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserUpdated(guid);

    public Task PublishUserDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserDeleted(guid);

    public Task PublishUserGroupCreated(Guid guid, string nameid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserGroupCreated(guid, nameid);

    public Task PublishUserGroupUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserGroupUpdated(guid);

    public Task PublishUserGroupDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnUserGroupDeleted(guid);

    public Task PublishPartitionCreated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnPartitionCreated(guid);

    public Task PublishPartitionUpdated(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnPartitionUpdated(guid);

    public Task PublishPartitionDeleted(Guid guid, IReadOnlyCollection<Guid> partitionGuids, CancellationToken ct = default)
        => Clients(partitionGuids).OnPartitionDeleted(guid);

    private IFargoEventClient Clients(IReadOnlyCollection<Guid> partitionGuids)
    {
        var groups = partitionGuids.Select(g => g.ToString()).Append("fargo-admin").ToList();
        return hub.Clients.Groups(groups);
    }
}
