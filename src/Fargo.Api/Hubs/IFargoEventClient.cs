namespace Fargo.Api.Hubs;

/// <summary>
/// Defines the methods the server can invoke on connected SignalR clients.
/// </summary>
public interface IFargoEventClient
{
    Task OnArticleCreated(Guid guid);
    Task OnArticleUpdated(Guid guid);
    Task OnArticleDeleted(Guid guid);

    Task OnItemCreated(Guid guid, Guid articleGuid);
    Task OnItemUpdated(Guid guid);
    Task OnItemDeleted(Guid guid);

    Task OnUserCreated(Guid guid, string nameid);
    Task OnUserUpdated(Guid guid);
    Task OnUserDeleted(Guid guid);

    Task OnUserGroupCreated(Guid guid, string nameid);
    Task OnUserGroupUpdated(Guid guid);
    Task OnUserGroupDeleted(Guid guid);

    Task OnPartitionCreated(Guid guid);
    Task OnPartitionUpdated(Guid guid);
    Task OnPartitionDeleted(Guid guid);
}
