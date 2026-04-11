namespace Fargo.Sdk.UserGroups;

/// <summary>Provides data for the <see cref="IUserGroupManager.Created"/> event.</summary>
public sealed class UserGroupCreatedEventArgs(Guid guid, string nameid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created user group.</summary>
    public Guid Guid { get; } = guid;

    /// <summary>Gets the name identifier of the created user group.</summary>
    public string Nameid { get; } = nameid;
}
