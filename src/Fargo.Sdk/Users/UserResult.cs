namespace Fargo.Sdk.Users;

/// <summary>
/// Represents a user returned by the API.
/// </summary>
/// <param name="Guid">The unique identifier of the user.</param>
/// <param name="Nameid">The unique name identifier used to log in.</param>
/// <param name="FirstName">The user's first name, or <see langword="null"/> if not set.</param>
/// <param name="LastName">The user's last name, or <see langword="null"/> if not set.</param>
/// <param name="Description">A short description of the user.</param>
/// <param name="DefaultPasswordExpirationPeriod">
/// The default period after which the user's password expires and must be changed.
/// </param>
/// <param name="RequirePasswordChangeAt">
/// The point in time at which the user will be required to change their password.
/// </param>
/// <param name="IsActive">Whether the user account is currently active.</param>
/// <param name="Permissions">The permissions assigned directly to this user.</param>
/// <param name="PartitionAccesses">The identifiers of partitions this user can access.</param>
public sealed record UserResult(
    Guid Guid,
    string Nameid,
    string? FirstName,
    string? LastName,
    string Description,
    TimeSpan DefaultPasswordExpirationPeriod,
    DateTimeOffset RequirePasswordChangeAt,
    bool IsActive,
    IReadOnlyCollection<PermissionResult> Permissions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    Guid? EditedByGuid = null
);
