namespace Fargo.Sdk.Authentication;

/// <summary>
/// Provides data for the <see cref="IAuthenticationManager.RefreshFailed"/> event.
/// </summary>
public sealed class RefreshFailedEventArgs
{
    internal RefreshFailedEventArgs(string? nameid, Exception exception)
    {
        Nameid = nameid;
        Exception = exception;
    }

    /// <summary>
    /// Gets the name identifier of the user whose refresh failed,
    /// or <see langword="null"/> if the session held no user information.
    /// </summary>
    public string? Nameid { get; }

    /// <summary>Gets the exception that caused the refresh failure.</summary>
    public Exception Exception { get; }
}
