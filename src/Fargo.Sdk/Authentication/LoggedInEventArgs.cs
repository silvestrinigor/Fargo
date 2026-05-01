namespace Fargo.Api.Authentication;

/// <summary>
/// Provides data for the <see cref="IAuthenticationManager.LoggedIn"/> event.
/// </summary>
public sealed class LoggedInEventArgs
{
    internal LoggedInEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    /// <summary>Gets the name identifier of the user who logged in.</summary>
    public string Nameid { get; }
}
