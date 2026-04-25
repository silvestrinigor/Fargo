using Fargo.Sdk.Events;

namespace Fargo.Sdk.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupEventSource"/>.</summary>
public sealed class UserGroupEventSource : IUserGroupEventSource
{
    public UserGroupEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, string>("OnUserGroupCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserGroupCreatedEventArgs(guid, nameid)));
    }

    public event EventHandler<UserGroupCreatedEventArgs>? Created;
}
