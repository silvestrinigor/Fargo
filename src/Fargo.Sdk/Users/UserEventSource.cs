using Fargo.Sdk.Events;

namespace Fargo.Sdk.Users;

/// <summary>Default implementation of <see cref="IUserEventSource"/>.</summary>
public sealed class UserEventSource : IUserEventSource
{
    /// <summary>Initializes a new instance.</summary>
    public UserEventSource(IFargoEventHub hub)
    {
        hub.On<Guid, string>("OnUserCreated", (guid, nameid) =>
            Created?.Invoke(this, new UserCreatedEventArgs(guid, nameid)));
    }

    /// <inheritdoc />
    public event EventHandler<UserCreatedEventArgs>? Created;
}
