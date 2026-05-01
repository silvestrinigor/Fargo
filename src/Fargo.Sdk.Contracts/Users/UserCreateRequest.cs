using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents the user payload inside a user create request.</summary>
public sealed record UserCreateDto(
    string Nameid,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    Guid? FirstPartition = null);

/// <summary>Represents a user create request.</summary>
public sealed record UserCreateRequest(UserCreateDto User);

/// <summary>Represents a user update request.</summary>
public sealed record UserUpdateRequest(
    string? Nameid = null,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    string? Password = null,
    bool? IsActive = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationPeriod = null);
