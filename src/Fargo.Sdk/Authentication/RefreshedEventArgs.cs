namespace Fargo.Api.Authentication;

/// <summary>
/// Provides data for the <see cref="IAuthenticationManager.Refreshed"/> event.
/// </summary>
public sealed class RefreshedEventArgs
{
    internal RefreshedEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    /// <summary>Gets the name identifier of the user whose token was refreshed.</summary>
    public string Nameid { get; }
}
