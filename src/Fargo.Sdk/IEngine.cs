using Fargo.Sdk.Authentication;

namespace Fargo.Sdk;

/// <summary>
/// Represents the main entry point for interacting with the Fargo SDK.
/// </summary>
public interface IEngine : IDisposable
{
    /// <summary>
    /// Gets the manager responsible for authentication operations,
    /// including login, logout, token refresh, and password changes.
    /// </summary>
    IAuthenticationManager AuthenticationManager { get; }
}
