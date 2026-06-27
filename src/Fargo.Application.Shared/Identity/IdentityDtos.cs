using Fargo.Core.Shared;
using Fargo.Core.Shared.Identity;

namespace Fargo.Application.Shared.Identity;

/// <summary>
/// Represents the result of a successful authentication.
/// </summary>
/// <param name="AccessToken">
/// The generated access token used to authenticate API requests.
/// </param>
/// <param name="RefreshToken">
/// The refresh token used to obtain a new access token when the current one expires.
/// </param>
/// <param name="ExpiresAt">
/// The date and time when the access token expires.
/// </param>
/// <param name="IsAdmin">
/// Whether the authenticated user has administrator privileges.
/// </param>
/// <param name="PermissionActions">
/// The effective set of actions the user is permitted to perform,
/// combining direct permissions and those inherited from user groups.
/// </param>
/// <param name="PartitionAccesses">
/// The effective set of partition identifiers the user can access,
/// including inherited group accesses and all descendant partitions.
/// </param>
public record AuthResult(
    Token AccessToken,
    Token RefreshToken,
    DateTimeOffset ExpiresAt,
    IReadOnlyCollection<ActionType> PermissionActions,
    IReadOnlyCollection<Guid> PartitionAccesses
);

public sealed record LoginDto(
    string Nameid,
    string Password
);

public sealed record RefreshDto(
    Token RefreshToken
);

public sealed record LogOutDto(
    Token RefreshToken
);
