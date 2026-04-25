using Fargo.Sdk.Events;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserEventSource"/>.</summary>
public sealed class UserEventSource : IUserEventSource
{
    public UserEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, string>("OnUserCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserCreatedEventArgs(guid, nameid)));
    }

    public event EventHandler<UserCreatedEventArgs>? Created;
}
