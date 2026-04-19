using Fargo.Domain;
using Fargo.Domain.Tokens;

namespace Fargo.Application.Authentication;

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
/// When true, <see cref="PermissionActions"/> and <see cref="PartitionAccesses"/> are empty
/// since admins bypass all permission and partition checks.
/// </param>
/// <param name="PermissionActions">
/// The effective set of actions the user is permitted to perform,
/// combining direct permissions and those inherited from user groups.
/// Empty for admin users.
/// </param>
/// <param name="PartitionAccesses">
/// The effective set of partition identifiers the user can access,
/// including inherited group accesses and all descendant partitions.
/// Empty for admin users.
/// </param>
public record AuthResult(
        Token AccessToken,
        Token RefreshToken,
        DateTimeOffset ExpiresAt,
        bool IsAdmin,
        IReadOnlyCollection<ActionType> PermissionActions,
        IReadOnlyCollection<Guid> PartitionAccesses
        );
