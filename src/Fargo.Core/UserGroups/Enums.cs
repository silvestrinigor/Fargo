namespace Fargo.Core.UserGroups;

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
