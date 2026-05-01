namespace Fargo.Api.Authentication;

/// <summary>Provides data for the <see cref="IAuthenticationService.Restored"/> event.</summary>
public sealed class SessionRestoredEventArgs(string nameid) : EventArgs
{
    /// <summary>Gets the restored user identifier.</summary>
    public string Nameid { get; } = nameid;
}
