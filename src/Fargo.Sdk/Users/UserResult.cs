namespace Fargo.Sdk.Users;

/// <summary>
/// Represents a user returned by the API.
/// </summary>
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
    IReadOnlyCollection<Guid> PartitionAccesses
);
