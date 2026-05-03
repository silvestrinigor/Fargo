namespace Fargo.Sdk.Contracts;

/// <summary>
/// Represents the set of actions that can be authorized in the system.
/// Integer values must stay aligned with the domain authorization model.
/// </summary>
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
    EditPartition = 16,
    AddBarcode = 17,
    RemoveBarcode = 18,
    CreateApiClient = 19,
    DeleteApiClient = 20,
    EditApiClient = 21,
}
