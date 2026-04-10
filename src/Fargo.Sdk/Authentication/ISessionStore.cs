namespace Fargo.Sdk.Authentication;

/// <summary>
/// Defines a persistence mechanism for storing and retrieving authentication sessions.
/// </summary>
/// <remarks>
/// Implement this interface and pass it to the <see cref="Engine"/> constructor to enable
/// automatic session persistence. When a store is configured, the SDK saves the session
/// after every successful login or token refresh, and can restore it via
/// <see cref="IAuthenticationManager.RestoreAsync"/>.
/// </remarks>
public interface ISessionStore
{
    /// <summary>
    /// Loads a previously saved session, or returns <see langword="null"/> if none exists.
    /// </summary>
    Task<StoredSession?> LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the specified session for future restoration.
    /// </summary>
    Task SaveAsync(StoredSession session, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes any stored session data.
    /// </summary>
    Task ClearAsync(CancellationToken cancellationToken = default);
}
