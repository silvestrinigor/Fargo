namespace Fargo.Sdk.Users;

/// <summary>Broadcasts the hub <c>OnUserCreated</c> event as a typed .NET event.</summary>
public interface IUserEventSource
{
    /// <summary>Raised when any authenticated client creates a user.</summary>
    event EventHandler<UserCreatedEventArgs>? Created;
}
