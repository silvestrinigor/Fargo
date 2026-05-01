namespace Fargo.Api.Authentication;

/// <summary>
/// Represents the current authentication session, exposing token data and the
/// permissions associated with the authenticated user.
/// </summary>
public interface IAuthSession
{
    /// <summary>Gets the name identifier of the authenticated user, or <see langword="null"/> if not authenticated.</summary>
    string? Nameid { get; }

    /// <summary>Gets the current access token, or <see langword="null"/> if not authenticated.</summary>
    string? AccessToken { get; }

    /// <summary>Gets the current refresh token, or <see langword="null"/> if not authenticated.</summary>
    string? RefreshToken { get; }

    /// <summary>Gets the UTC time at which the access token expires, or <see langword="null"/> if not authenticated.</summary>
    DateTimeOffset? ExpiresAt { get; }

    /// <summary>Gets a value indicating whether the session holds a valid token.</summary>
    bool IsAuthenticated { get; }

    /// <summary>Gets a value indicating whether the current access token has passed its expiry time.</summary>
    bool IsExpired { get; }

    /// <summary>Gets a value indicating whether the authenticated user has administrative privileges.</summary>
    bool IsAdmin { get; }

    /// <summary>Gets the set of action permissions granted to the authenticated user.</summary>
    IReadOnlyCollection<ActionType> PermissionActions { get; }

    /// <summary>Gets the identifiers of partitions the authenticated user can access.</summary>
    IReadOnlyCollection<Guid> PartitionAccesses { get; }

    /// <summary>
    /// Returns <see langword="true"/> if the authenticated user has the specified action permission.
    /// </summary>
    bool HasActionPermission(ActionType action);

    /// <summary>
    /// Returns <see langword="true"/> if the authenticated user has access to the specified partition.
    /// </summary>
    bool HasPartitionAccess(Guid partitionGuid);
}
