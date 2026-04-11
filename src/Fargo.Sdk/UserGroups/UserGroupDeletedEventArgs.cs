namespace Fargo.Sdk.UserGroups;

/// <summary>Provides data for the <see cref="IUserGroupManager.Deleted"/> event.</summary>
public sealed class UserGroupDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted user group.</summary>
    public Guid Guid { get; } = guid;
}
