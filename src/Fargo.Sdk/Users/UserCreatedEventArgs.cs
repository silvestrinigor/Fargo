namespace Fargo.Sdk.Users;

/// <summary>Provides data for the <see cref="IUserEventSource.Created"/> event.</summary>
public sealed class UserCreatedEventArgs(Guid guid, string nameid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created user.</summary>
    public Guid Guid { get; } = guid;

    /// <summary>Gets the login name identifier of the created user.</summary>
    public string Nameid { get; } = nameid;
}
