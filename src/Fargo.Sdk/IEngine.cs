using Fargo.Sdk.Authentication;
using Fargo.Sdk.UserGroups;
using Fargo.Sdk.Users;

namespace Fargo.Sdk;

/// <summary>
/// Represents the main entry point for interacting with the Fargo SDK.
/// </summary>
public interface IEngine : IDisposable
{
    /// <summary>Gets the manager responsible for authentication operations.</summary>
    IAuthenticationManager Authentication { get; }

    /// <summary>Gets the client for user operations.</summary>
    IUserClient Users { get; }

    /// <summary>Gets the client for user group operations.</summary>
    IUserGroupClient UserGroups { get; }

    /// <summary>
    /// Configures the server URL without performing any authentication.
    /// Use this in hosted scenarios where the server address is known upfront
    /// (e.g. read from configuration) and authentication is managed separately
    /// via <see cref="IAuthenticationManager"/>.
    /// </summary>
    void Configure(string server);

    /// <summary>
    /// Authenticates the user, switching to the specified server first.
    /// Logs out first if already authenticated.
    /// </summary>
    Task LogInAsync(string server, string nameid, string password, CancellationToken cancellationToken = default);

    /// <summary>
    /// Ends the current authenticated session.
    /// </summary>
    Task LogOutAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts to restore a previously saved session from the configured <see cref="Authentication.ISessionStore"/>.
    /// Sets the server URL and refreshes the token automatically if it has expired.
    /// Returns <see langword="false"/> if no session store is configured or no saved session exists.
    /// </summary>
    Task<bool> RestoreSessionAsync(string server, CancellationToken cancellationToken = default);
}
