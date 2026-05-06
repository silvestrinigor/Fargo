using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents the user payload inside a user create request.</summary>
public sealed record UserCreateRequest(
    string Nameid,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateRequest>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    IReadOnlyCollection<Guid>? Partitions = null,
    IReadOnlyCollection<Guid>? UserGroups = null);
