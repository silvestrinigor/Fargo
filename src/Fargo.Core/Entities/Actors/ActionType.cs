namespace Fargo.Core.Entities.Actors;

public enum ActionType
{
    CreateArticle = 1,
    DeleteArticle = 2,
    EditArticle = 3,
    CreateItem = 4,
    DeleteItem = 5,
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
    ActivateServiceAccount = 33
}
