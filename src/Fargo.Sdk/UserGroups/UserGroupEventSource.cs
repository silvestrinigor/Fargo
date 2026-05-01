using Fargo.Api.Events;

namespace Fargo.Api.UserGroups;

/// <summary>Default implementation of <see cref="IUserGroupEventSource"/>.</summary>
public sealed class UserGroupEventSource : IUserGroupEventSource
{
    /// <summary>Initializes a new instance.</summary>
    public UserGroupEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, string>("OnUserGroupCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserGroupCreatedEventArgs(guid, nameid)));
    }

    /// <inheritdoc />
    public event EventHandler<UserGroupCreatedEventArgs>? Created;
}
