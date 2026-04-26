namespace Fargo.Sdk.UserGroups;

/// <summary>Provides data for the <see cref="UserGroup.Updated"/> event.</summary>
public sealed class UserGroupUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated user group.</summary>
    public Guid Guid { get; } = guid;
}
