using Fargo.Sdk.Authentication;

namespace Fargo.Sdk;

/// <summary>
/// Represents the main entry point for interacting with the Fargo SDK.
/// </summary>
public interface IEngine : IDisposable
{
    /// <summary>Gets the manager responsible for authentication operations.</summary>
    IAuthenticationManager Authentication { get; }

    /// <summary>
    /// Authenticates the user, switching to the specified server first.
    /// Logs out first if already authenticated.
    /// </summary>
    Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current authenticated session.
    /// </summary>
    Task LogOutAsync(CancellationToken cancellationToken = default);
}
