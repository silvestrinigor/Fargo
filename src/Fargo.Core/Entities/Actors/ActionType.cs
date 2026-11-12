namespace Fargo.Core.Entities.Actors;

public enum ActionType
{
    CreateArticle = 1,
    DeleteArticle = 2,
    EditArticle = 3,
    CreateItem = 4,
    DeleteItem = 5,

    /// <summary>
    /// No operation requires it any more: moving an item and changing its partitions have their own
    /// permissions. The value stays because roles already store it.
    /// </summary>
    EditItem = 6,
    CreateUser = 7,
    DeleteUser = 8,
    EditUser = 9,
    ChangeAnotherUserPassword = 10,
    CreateUserGroup = 11,
    DeleteUserGroup = 12,
    EditUserGroup = 13,
    ChangeUserGroupMembers = 14,
    CreatePartition = 15,
    DeletePartition = 16,
    EditPartition = 17,
    ReadAuditLogs = 18,
    ReadItemEvents = 19,
    CreateServiceAccount = 20,
    DeleteServiceAccount = 21,

    /// <summary>
    /// Also covers rotating the service account's secret.
    /// </summary>
    EditServiceAccount = 22,

    ReadServiceAccount = 23,
    CreateSupplyOrder = 24,

    /// <summary>
    /// Covers draft lines, description and receiving container.
    /// </summary>
    EditSupplyOrder = 25,

    PlaceSupplyOrder = 26,
    ReceiveSupplyOrder = 27,
    CancelSupplyOrder = 28,
    CloseSupplyOrder = 29,
    ReadSupplyOrders = 30,

    /// <summary>
    /// Activates or deactivates a user.
    /// </summary>
    ActivateUser = 31,

    /// <summary>
    /// Activates or deactivates a user group.
    /// </summary>
    ActivateUserGroup = 32,

    /// <summary>
    /// Activates or deactivates a service account.
    /// </summary>
    ActivateServiceAccount = 33,

    /// <summary>
    /// Places an item inside a container or removes it from its container.
    /// </summary>
    MoveItem = 34,

    /// <summary>
    /// Grants and revokes the direct permissions of a user.
    /// </summary>
    ChangeUserPermissions = 35,

    /// <summary>
    /// Grants and revokes the permissions of a user group, which every member inherits.
    /// </summary>
    ChangeUserGroupPermissions = 36,

    /// <summary>
    /// Grants and revokes the permissions of a service account.
    /// </summary>
    ChangeServiceAccountPermissions = 37,

    /// <summary>
    /// Changes the partitions a user belongs to and the partitions it can access.
    /// </summary>
    ChangeUserPartitions = 38,

    /// <summary>
    /// Changes the partitions a user group belongs to and the partitions it can access.
    /// </summary>
    ChangeUserGroupPartitions = 39,

    /// <summary>
    /// Changes the partitions a service account belongs to and the partitions it can access.
    /// </summary>
    ChangeServiceAccountPartitions = 40,

    /// <summary>
    /// Sets or clears the parent of a user group, which passes the parent's permissions down to the group's members.
    /// </summary>
    ChangeUserGroupParent = 41,

    /// <summary>
    /// Moves a partition under another parent, which changes who can access it.
    /// </summary>
    ChangePartitionParent = 42,

    /// <summary>
    /// Replaces a service account's secret. Separate from <see cref="EditServiceAccount"/> so it can be granted on its own.
    /// </summary>
    RotateServiceAccountSecret = 43,

    /// <summary>
    /// Changes the partitions an article belongs to, which decides who can see and use it.
    /// </summary>
    ChangeArticlePartitions = 44,

    /// <summary>
    /// Changes the partitions an item belongs to, which decides who can see and use it.
    /// </summary>
    ChangeItemPartitions = 45
}
