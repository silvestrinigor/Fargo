using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents a user returned by the API.</summary>
public sealed record UserDto(
    Guid Guid,
    string Nameid,
    string? FirstName,
    string? LastName,
    string Description,
    TimeSpan DefaultPasswordExpirationPeriod,
    DateTimeOffset RequirePasswordChangeAt,
    bool IsActive,
    IReadOnlyCollection<PermissionDto> Permissions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    Guid? EditedByGuid = null);
