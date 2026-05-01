namespace Fargo.Api.Users;

/// <summary>Provides data for the <see cref="User.Updated"/> event.</summary>
public sealed class UserUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated user.</summary>
    public Guid Guid { get; } = guid;
}
