namespace Fargo.Domain;

// TODO: Move to a new project Fargo.Types to remove the duplicated code in the sdk.
/// <summary>
/// Represents the set of actions that can be authorized in the system.
///
/// Each value defines a specific permission that can be granted
/// to a user through <see cref="Fargo.Domain.Users.UserPermission"/>.
/// </summary>
public enum ActionType
{
    /// <summary>
    /// Allows creating new articles.
    /// </summary>
    CreateArticle = 0,

    /// <summary>
    /// Allows deleting existing articles.
    /// </summary>
    DeleteArticle = 1,

    /// <summary>
    /// Allows editing existing articles.
    /// </summary>
    EditArticle = 2,

    /// <summary>
    /// Allows creating new items associated with articles.
    /// </summary>
    CreateItem = 3,

    /// <summary>
    /// Allows deleting existing items.
    /// </summary>
    DeleteItem = 4,

    /// <summary>
    /// Allows editing existing items.
    /// </summary>
    EditItem = 5,

    /// <summary>
    /// Allows creating new users.
    /// </summary>
    CreateUser = 6,

    /// <summary>
    /// Allows deleting existing users.
    /// </summary>
    DeleteUser = 7,

    /// <summary>
    /// Allows editing existing users.
    /// </summary>
    EditUser = 8,

    /// <summary>
    /// Allows changing the password of another user without requiring
    /// the current password of that user.
    ///
    /// This permission is typically intended for administrative users
    /// who need to reset another user's password. It does not apply to
    /// a user changing their own password, which should require the
    /// current password.
    /// </summary>
    ChangeOtherUserPassword = 9,

    /// <summary>
    /// Allows creating new user groups.
    /// </summary>
    CreateUserGroup = 10,

    /// <summary>
    /// Allows deleting existing user groups.
    /// </summary>
    DeleteUserGroup = 11,

    /// <summary>
    /// Allows editing existing user groups.
    /// </summary>
    EditUserGroup = 12,

    /// <summary>
    /// Allows modifying the membership of a user group,
    /// including adding or removing users from the group.
    ///
    /// This permission controls operations that change which
    /// users belong to a given <see cref="Fargo.Domain.Users.UserGroup"/>.
    /// It does not grant permission to create, delete, or edit the group
    /// itself, which are controlled by <see cref="CreateUserGroup"/>,
    /// <see cref="DeleteUserGroup"/>, and <see cref="EditUserGroup"/>.
    /// </summary>
    ChangeUserGroupMembers = 13,

    /// <summary>
    /// Allows creating partitions.
    /// </summary>
    CreatePartition = 14,

    /// <summary>
    /// Allows deleting existing partitions.
    /// </summary>
    DeletePartition = 15,

    /// <summary>
    /// Allows editing existing partitions.
    /// </summary>
    EditPartition = 16,

    /// <summary>
    /// Allows adding barcodes to articles.
    /// </summary>
    AddBarcode = 17,

    /// <summary>
    /// Allows removing barcodes from articles.
    /// </summary>
    RemoveBarcode = 18,

    /// <summary>
    /// Allows creating new API clients.
    /// </summary>
    CreateApiClient = 19,

    /// <summary>
    /// Allows deleting existing API clients.
    /// </summary>
    DeleteApiClient = 20,

    /// <summary>
    /// Allows editing existing API clients.
    /// </summary>
    EditApiClient = 21,
}
