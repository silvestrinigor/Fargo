namespace Fargo.Api.Authentication;

/// <summary>
/// Provides data for the <see cref="IAuthenticationManager.PasswordChanged"/> event.
/// </summary>
public sealed class PasswordChangedEventArgs
{
    internal PasswordChangedEventArgs(string nameid)
    {
        Nameid = nameid;
    }

    /// <summary>Gets the name identifier of the user whose password was changed.</summary>
    public string Nameid { get; }
}
