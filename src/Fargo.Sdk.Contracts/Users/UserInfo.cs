using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.Users;

/// <summary>Represents the user areas changed by the last edit operation.</summary>
[Flags]
public enum UserModifiedType
{
    None = 0,
    General = 1 << 0,
    PasswordChanged = 1 << 1,
    PermissionsChanged = 1 << 2,
    PartitionsChanged = 1 << 3,
    UserGroupsChanged = 1 << 4,
    Activated = 1 << 5,
    Deactivated = 1 << 6,
}

/// <summary>Represents a user returned by the API.</summary>
public sealed record UserInfo(
    Guid Guid,
    string Nameid,
    string? FirstName,
    string? LastName,
    string Description,
    TimeSpan? DefaultPasswordExpirationPeriod,
    DateTimeOffset? RequirePasswordChangeAt,
    bool IsActive,
    IReadOnlyCollection<PermissionInfo> Permissions,
    IReadOnlyCollection<Guid> PartitionAccesses,
    IReadOnlyCollection<Guid> UserGroups,
    Guid? EditedByGuid = null,
    UserModifiedType ModificationTypes = UserModifiedType.None);
