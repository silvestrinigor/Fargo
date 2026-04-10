namespace Fargo.Sdk.Authentication;

/// <summary>
/// Provides data for the <see cref="IAuthenticationManager.LoggedOut"/> event.
/// </summary>
public sealed class LoggedOutEventArgs
{
    internal LoggedOutEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    /// <summary>Gets the name identifier of the user who logged out.</summary>
    public string Nameid { get; }
}
