namespace Fargo.Core.Shared;

/// <summary>
/// Represents the set of actions in the system.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Create article.
    /// </summary>
    CreateArticle = 1,

    /// <summary>
    /// Delete article.
    /// </summary>
    DeleteArticle = 2,

    /// <summary>
    /// Edit article.
    /// </summary>
    EditArticle = 3,

    /// <summary>
    /// Create item.
    /// </summary>
    CreateItem = 4,

    /// <summary>
    /// Delete item.
    /// </summary>
    DeleteItem = 5,

    /// <summary>
    /// Edit item.
    /// </summary>
    EditItem = 6,

    /// <summary>
    /// Create user.
    /// </summary>
    CreateUser = 7,

    /// <summary>
    /// Delete user.
    /// </summary>
    DeleteUser = 8,

    /// <summary>
    /// Edit user.
    /// </summary>
    EditUser = 9,

    /// <summary>
    /// Change another user's password.
    /// </summary>
    ChangeAnotherUserPassword = 10,

    /// <summary>
    /// Create user group.
    /// </summary>
    CreateUserGroup = 11,

    /// <summary>
    /// Delete user group.
    /// </summary>
    DeleteUserGroup = 12,

    /// <summary>
    /// Edit user group.
    /// </summary>
    EditUserGroup = 13,

    /// <summary>
    /// Change user group members.
    /// </summary>
    ChangeUserGroupMembers = 14,

    /// <summary>
    /// Create partition.
    /// </summary>
    CreatePartition = 15,

    /// <summary>
    /// Delete partition.
    /// </summary>
    DeletePartition = 16,

    /// <summary>
    /// Edit partition.
    /// </summary>
    EditPartition = 17,
}
