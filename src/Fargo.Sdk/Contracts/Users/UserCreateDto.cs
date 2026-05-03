using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents the user payload inside a user create request.</summary>
public sealed record UserCreateDto(
    string Nameid,
    string Password,
    string? FirstName = null,
    string? LastName = null,
    string? Description = null,
    IReadOnlyCollection<PermissionUpdateDto>? Permissions = null,
    TimeSpan? DefaultPasswordExpirationTimeSpan = null,
    Guid? FirstPartition = null);
