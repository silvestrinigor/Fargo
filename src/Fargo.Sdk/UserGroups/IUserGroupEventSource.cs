namespace Fargo.Sdk.UserGroups;

/// <summary>Broadcasts the hub <c>OnUserGroupCreated</c> event as a typed .NET event.</summary>
public interface IUserGroupEventSource
{
    /// <summary>Raised when any authenticated client creates a user group.</summary>
    event EventHandler<UserGroupCreatedEventArgs>? Created;
}
