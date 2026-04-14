namespace Fargo.Domain.Enums;

// TODO: add the enum type numbers
/// <summary>
/// Represents the set of actions that can be authorized in the system.
///
/// Each value defines a specific permission that can be granted
/// to a user through <see cref="Entities.UserPermission"/>.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Allows creating new articles.
    /// </summary>
    CreateArticle,

    /// <summary>
    /// Allows deleting existing articles.
    /// </summary>
    DeleteArticle,

    /// <summary>
    /// Allows editing existing articles.
    /// </summary>
    EditArticle,

    /// <summary>
    /// Allows creating new items associated with articles.
    /// </summary>
    CreateItem,

    /// <summary>
    /// Allows deleting existing items.
    /// </summary>
    DeleteItem,

    /// <summary>
    /// Allows editing existing items.
    /// </summary>
    EditItem,

    /// <summary>
    /// Allows creating new users.
    /// </summary>
    CreateUser,

    /// <summary>
    /// Allows deleting existing users.
    /// </summary>
    DeleteUser,

    /// <summary>
    /// Allows editing existing users.
    /// </summary>
    EditUser,

    /// <summary>
    /// Allows changing the password of another user without requiring
    /// the current password of that user.
    ///
    /// This permission is typically intended for administrative users
    /// who need to reset another user's password. It does not apply to
    /// a user changing their own password, which should require the
    /// current password.
    /// </summary>
    ChangeOtherUserPassword,

    /// <summary>
    /// Allows creating new user groups.
    /// </summary>
    CreateUserGroup,

    /// <summary>
    /// Allows deleting existing user groups.
    /// </summary>
    DeleteUserGroup,

    /// <summary>
    /// Allows editing existing user groups.
    /// </summary>
    EditUserGroup,

    /// <summary>
    /// Allows modifying the membership of a user group,
    /// including adding or removing users from the group.
    ///
    /// This permission controls operations that change which
    /// users belong to a given <see cref="Entities.UserGroup"/>.
    /// It does not grant permission to create, delete, or edit the group
    /// itself, which are controlled by <see cref="CreateUserGroup"/>,
    /// <see cref="DeleteUserGroup"/>, and <see cref="EditUserGroup"/>.
    /// </summary>
    ChangeUserGroupMembers,

    /// <summary>
    /// Allows creating partitions.
    /// </summary>
    CreatePartition,

    /// <summary>
    /// Allows deleting existing partitions.
    /// </summary>
    DeletePartition,

    /// <summary>
    /// Allows editing existing partitions.
    /// </summary>
    EditPartition,

    /// <summary>
    /// Allows adding barcodes to articles.
    /// </summary>
    AddBarcode,

    /// <summary>
    /// Allows removing barcodes from articles.
    /// </summary>
    RemoveBarcode,
}
