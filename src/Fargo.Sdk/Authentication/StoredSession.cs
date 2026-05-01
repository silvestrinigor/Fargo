namespace Fargo.Api.Authentication;

/// <summary>
/// A serialisable snapshot of an authentication session, used by <see cref="ISessionStore"/>
/// to persist and restore sessions across application restarts.
/// </summary>
/// <param name="Nameid">The name identifier of the authenticated user.</param>
/// <param name="AccessToken">The JWT access token.</param>
/// <param name="RefreshToken">The refresh token used to obtain a new access token when the current one expires.</param>
/// <param name="ExpiresAt">The UTC time at which the access token expires.</param>
/// <param name="IsAdmin">Whether the user has administrative privileges.</param>
/// <param name="PermissionActions">The set of action permissions granted to the user.</param>
/// <param name="PartitionAccesses">The identifiers of partitions the user can access.</param>
public sealed record StoredSession(
    string Nameid,
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
