namespace Fargo.HttpContracts;

public enum ActionType
{
    CreateArticle = 0,
    DeleteArticle = 1,
    EditArticle = 2,
    CreateItem = 3,
    DeleteItem = 4,
    EditItem = 5,
    CreateUser = 6,
    DeleteUser = 7,
    EditUser = 8,
    ChangeOtherUserPassword = 9,
    CreateUserGroup = 10,
    DeleteUserGroup = 11,
    EditUserGroup = 12,
    ChangeUserGroupMembers = 13,
    CreatePartition = 14,
    DeletePartition = 15,
    EditPartition = 16
}

public enum ArticleType
{
    Default = 1,
    Variation = 2,
    Pack = 3,
    Kit = 4,
    Container = 5
}

public enum BarcodeFormat
{
    Ean13 = 0,
    Ean8 = 1,
    UpcA = 2,
    UpcE = 3,
    Code128 = 4,
    Code39 = 5,
    Itf14 = 6,
    Gs1128 = 7,
    QrCode = 8,
    DataMatrix = 9
}

[Flags]
public enum ArticleModifiedType
{
    None = 0,
    General = 1 << 0,
    MetricsChanged = 1 << 1,
    BarcodesChanged = 1 << 2,
    PartitionsChanged = 1 << 3,
    Container = 1 << 4,
    Relation = 1 << 5,
    Activated = 1 << 6,
    Deactivated = 1 << 7
}

[Flags]
public enum ItemModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentContainerChanged = 1 << 1,
    PartitionsChanged = 1 << 2,
    Activated = 1 << 3,
    Deactivated = 1 << 4
}

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
    Deactivated = 1 << 6
}

[Flags]
public enum UserGroupModifiedType
{
    None = 0,
    General = 1 << 0,
    PermissionsChanged = 1 << 1,
    PartitionsChanged = 1 << 2,
    Activated = 1 << 3,
    Deactivated = 1 << 4
}

[Flags]
public enum PartitionModifiedType
{
    None = 0,
    General = 1 << 0,
    ParentPartitionChanged = 1 << 1,
    Activated = 1 << 2,
    Deactivated = 1 << 3
}
