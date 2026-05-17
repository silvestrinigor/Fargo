using Fargo.Sdk.Contracts.Permissions;

namespace Fargo.Sdk.Contracts.UserGroups;

/// <summary>Represents the user group areas changed by the last edit operation.</summary>
[Flags]
public enum UserGroupModifiedType
{
    None = 0,
    General = 1 << 0,
    PermissionsChanged = 1 << 1,
    PartitionsChanged = 1 << 2,
    Activated = 1 << 3,
    Deactivated = 1 << 4,
}

/// <summary>Represents a user group returned by the API.</summary>
public sealed record UserGroupInfo(
    Guid Guid,
    string Nameid,
    string Description,
    bool IsActive,
    IReadOnlyCollection<PermissionInfo> Permissions,
    IReadOnlyCollection<Guid> Partitions,
    Guid? EditedByGuid = null,
    UserGroupModifiedType ModificationTypes = UserGroupModifiedType.None);
