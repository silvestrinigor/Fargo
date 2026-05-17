namespace Fargo.Core.Users;

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
