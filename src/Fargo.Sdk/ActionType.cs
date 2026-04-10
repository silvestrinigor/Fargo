namespace Fargo.Sdk;

/// <summary>
/// Represents the set of actions that can be authorized in the system.
/// Mirrors <c>Fargo.Domain.Enums.ActionType</c> — integer values and member order
/// must remain in sync with the domain enum.
/// </summary>
public enum ActionType
{
    CreateArticle,
    DeleteArticle,
    EditArticle,
    CreateItem,
    DeleteItem,
    EditItem,
    CreateUser,
    DeleteUser,
    EditUser,
    ChangeOtherUserPassword,
    CreateUserGroup,
    DeleteUserGroup,
    EditUserGroup,
    ChangeUserGroupMembers,
    CreatePartition,
    DeletePartition,
    EditPartition,
}
