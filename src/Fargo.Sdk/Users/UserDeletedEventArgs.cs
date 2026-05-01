namespace Fargo.Api.Users;

/// <summary>Provides data for the <see cref="User.Deleted"/> event.</summary>
public sealed class UserDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted user.</summary>
    public Guid Guid { get; } = guid;
}
