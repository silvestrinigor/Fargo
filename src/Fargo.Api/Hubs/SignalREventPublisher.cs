using Fargo.Application.Events;
using Microsoft.AspNetCore.SignalR;

namespace Fargo.Api.Hubs;

/// <summary>
/// Implements <see cref="IFargoEventPublisher"/> by broadcasting events
/// to all connected SignalR clients via <see cref="FargoEventHub"/>.
/// </summary>
public sealed class SignalREventPublisher(IHubContext<FargoEventHub, IFargoEventClient> hub) : IFargoEventPublisher
{
    public Task PublishArticleCreated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnArticleCreated(guid);

    public Task PublishArticleUpdated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnArticleUpdated(guid);

    public Task PublishArticleDeleted(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnArticleDeleted(guid);

    public Task PublishItemCreated(Guid guid, Guid articleGuid, CancellationToken ct = default)
        => hub.Clients.All.OnItemCreated(guid, articleGuid);

    public Task PublishItemUpdated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnItemUpdated(guid);

    public Task PublishItemDeleted(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnItemDeleted(guid);

    public Task PublishUserCreated(Guid guid, string nameid, CancellationToken ct = default)
        => hub.Clients.All.OnUserCreated(guid, nameid);

    public Task PublishUserUpdated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnUserUpdated(guid);

    public Task PublishUserDeleted(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnUserDeleted(guid);

    public Task PublishUserGroupCreated(Guid guid, string nameid, CancellationToken ct = default)
        => hub.Clients.All.OnUserGroupCreated(guid, nameid);

    public Task PublishUserGroupUpdated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnUserGroupUpdated(guid);

    public Task PublishUserGroupDeleted(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnUserGroupDeleted(guid);

    public Task PublishPartitionCreated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnPartitionCreated(guid);

    public Task PublishPartitionUpdated(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnPartitionUpdated(guid);

    public Task PublishPartitionDeleted(Guid guid, CancellationToken ct = default)
        => hub.Clients.All.OnPartitionDeleted(guid);
}
