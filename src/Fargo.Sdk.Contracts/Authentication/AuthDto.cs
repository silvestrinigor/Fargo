namespace Fargo.Sdk.Contracts.Authentication;

/// <summary>
/// The token and permission data returned by the server after a successful authentication
/// or token refresh.
/// </summary>
/// <param name="AccessToken">The JWT access token to include in subsequent API requests.</param>
/// <param name="RefreshToken">The token used to obtain a new access token when the current one expires.</param>
/// <param name="ExpiresAt">The UTC time at which the access token expires.</param>
/// <param name="IsAdmin">Whether the authenticated user has administrative privileges.</param>
/// <param name="PermissionActions">The set of action permissions granted to the user.</param>
/// <param name="PartitionAccesses">The identifiers of partitions the user can access.</param>
public sealed record AuthDto(
    string AccessToken,
    string RefreshToken,
    DateTimeOffset ExpiresAt,
    bool IsAdmin,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses
);
