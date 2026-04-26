namespace Fargo.Sdk;

/// <summary>
/// Represents the set of actions that can be authorized in the system.
/// Mirrors <c>Fargo.Domain.Enums.ActionType</c> — integer values and member order
/// must remain in sync with the domain enum.
/// </summary>
public enum ActionType
{
    /// <summary>Permission to create articles.</summary>
    CreateArticle = 0,
    /// <summary>Permission to delete articles.</summary>
    DeleteArticle = 1,
    /// <summary>Permission to edit articles.</summary>
    EditArticle = 2,
    /// <summary>Permission to create items.</summary>
    CreateItem = 3,
    /// <summary>Permission to delete items.</summary>
    DeleteItem = 4,
    /// <summary>Permission to edit items.</summary>
    EditItem = 5,
    /// <summary>Permission to create users.</summary>
    CreateUser = 6,
    /// <summary>Permission to delete users.</summary>
    DeleteUser = 7,
    /// <summary>Permission to edit users.</summary>
    EditUser = 8,
    /// <summary>Permission to change another user's password.</summary>
    ChangeOtherUserPassword = 9,
    /// <summary>Permission to create user groups.</summary>
    CreateUserGroup = 10,
    /// <summary>Permission to delete user groups.</summary>
    DeleteUserGroup = 11,
    /// <summary>Permission to edit user groups.</summary>
    EditUserGroup = 12,
    /// <summary>Permission to add or remove users from user groups.</summary>
    ChangeUserGroupMembers = 13,
    /// <summary>Permission to create partitions.</summary>
    CreatePartition = 14,
    /// <summary>Permission to delete partitions.</summary>
    DeletePartition = 15,
    /// <summary>Permission to edit partitions.</summary>
    EditPartition = 16,
    /// <summary>Permission to add barcodes to articles.</summary>
    AddBarcode = 17,
    /// <summary>Permission to remove barcodes from articles.</summary>
    RemoveBarcode = 18,
    /// <summary>Permission to create API clients.</summary>
    CreateApiClient = 19,
    /// <summary>Permission to delete API clients.</summary>
    DeleteApiClient = 20,
    /// <summary>Permission to edit API clients.</summary>
    EditApiClient = 21,
}
