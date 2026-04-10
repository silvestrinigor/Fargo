namespace Fargo.Sdk;

/// <summary>
/// Represents the set of actions that can be authorized in the system.
/// Mirrors <c>Fargo.Domain.Enums.ActionType</c> — integer values and member order
/// must remain in sync with the domain enum.
/// </summary>
public enum ActionType
{
    /// <summary>Permission to create articles.</summary>
    CreateArticle,
    /// <summary>Permission to delete articles.</summary>
    DeleteArticle,
    /// <summary>Permission to edit articles.</summary>
    EditArticle,
    /// <summary>Permission to create items.</summary>
    CreateItem,
    /// <summary>Permission to delete items.</summary>
    DeleteItem,
    /// <summary>Permission to edit items.</summary>
    EditItem,
    /// <summary>Permission to create users.</summary>
    CreateUser,
    /// <summary>Permission to delete users.</summary>
    DeleteUser,
    /// <summary>Permission to edit users.</summary>
    EditUser,
    /// <summary>Permission to change another user's password.</summary>
    ChangeOtherUserPassword,
    /// <summary>Permission to create user groups.</summary>
    CreateUserGroup,
    /// <summary>Permission to delete user groups.</summary>
    DeleteUserGroup,
    /// <summary>Permission to edit user groups.</summary>
    EditUserGroup,
    /// <summary>Permission to add or remove users from user groups.</summary>
    ChangeUserGroupMembers,
    /// <summary>Permission to create partitions.</summary>
    CreatePartition,
    /// <summary>Permission to delete partitions.</summary>
    DeletePartition,
    /// <summary>Permission to edit partitions.</summary>
    EditPartition,
}
