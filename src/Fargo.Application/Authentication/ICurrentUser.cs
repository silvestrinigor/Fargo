namespace Fargo.Application.Security;

/// <summary>
/// Provides information about the currently authenticated user.
/// </summary>
/// <remarks>
/// This abstraction allows the application layer to access
/// user identity information without depending on HTTP,
/// authentication frameworks, or infrastructure concerns.
/// </remarks>
public interface ICurrentUser
{
    /// <summary>
    /// Gets the unique identifier of the current user.
    /// </summary>
    /// <remarks>
    /// Returns <see cref="Guid.Empty"/> when the user is not authenticated.
    /// </remarks>
    Guid UserGuid
    {
        get;
    }

    /// <summary>
    /// Gets a value indicating whether the current user is authenticated.
    /// </summary>
    bool IsAuthenticated
    {
        get;
    }
}
