using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents a user update request.</summary>
public sealed record UserUpdateDto(
    string? Nameid = null,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    string? Password = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null);
