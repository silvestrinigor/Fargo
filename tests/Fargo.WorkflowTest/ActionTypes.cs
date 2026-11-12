namespace Fargo.WorkflowTest;

/// <summary>
/// The permission action codes as exposed by the API. These mirror
/// <c>Fargo.Core.Entities.Actors.ActionType</c>; the generated client transmits them as
/// plain integers (<c>List&lt;int?&gt;</c>), so the workflow references them by
/// these named constants for readability.
/// </summary>
internal static class ActionTypes
{
    public const int CreateArticle = 1;
    public const int DeleteArticle = 2;
    public const int EditArticle = 3;
    public const int CreateItem = 4;
    public const int DeleteItem = 5;
    public const int EditItem = 6;
    public const int CreateUser = 7;
    public const int DeleteUser = 8;
    public const int EditUser = 9;
    public const int ChangeAnotherUserPassword = 10;
    public const int CreateUserGroup = 11;
    public const int DeleteUserGroup = 12;
    public const int EditUserGroup = 13;
    public const int ChangeUserGroupMembers = 14;
    public const int CreatePartition = 15;
    public const int DeletePartition = 16;
    public const int EditPartition = 17;
    public const int ReadAuditLogs = 18;
    public const int ReadItemEvents = 19;
    public const int CreateServiceAccount = 20;
    public const int DeleteServiceAccount = 21;
    public const int EditServiceAccount = 22;
    public const int ReadServiceAccount = 23;
    public const int CreateSupplyOrder = 24;
    public const int EditSupplyOrder = 25;
    public const int PlaceSupplyOrder = 26;
    public const int ReceiveSupplyOrder = 27;
    public const int CancelSupplyOrder = 28;
    public const int CloseSupplyOrder = 29;
    public const int ReadSupplyOrders = 30;
    public const int ActivateUser = 31;
    public const int ActivateUserGroup = 32;
    public const int ActivateServiceAccount = 33;

    public static string Name(int code) => code switch
    {
        CreateArticle => "CreateArticle",
        DeleteArticle => "DeleteArticle",
        EditArticle => "EditArticle",
        CreateItem => "CreateItem",
        DeleteItem => "DeleteItem",
        EditItem => "EditItem",
        CreateUser => "CreateUser",
        DeleteUser => "DeleteUser",
        EditUser => "EditUser",
        ChangeAnotherUserPassword => "ChangeAnotherUserPassword",
        CreateUserGroup => "CreateUserGroup",
        DeleteUserGroup => "DeleteUserGroup",
        EditUserGroup => "EditUserGroup",
        ChangeUserGroupMembers => "ChangeUserGroupMembers",
        CreatePartition => "CreatePartition",
        DeletePartition => "DeletePartition",
        EditPartition => "EditPartition",
        ReadAuditLogs => "ReadAuditLogs",
        ReadItemEvents => "ReadItemEvents",
        CreateServiceAccount => "CreateServiceAccount",
        DeleteServiceAccount => "DeleteServiceAccount",
        EditServiceAccount => "EditServiceAccount",
        ReadServiceAccount => "ReadServiceAccount",
        CreateSupplyOrder => "CreateSupplyOrder",
        EditSupplyOrder => "EditSupplyOrder",
        PlaceSupplyOrder => "PlaceSupplyOrder",
        ReceiveSupplyOrder => "ReceiveSupplyOrder",
        CancelSupplyOrder => "CancelSupplyOrder",
        CloseSupplyOrder => "CloseSupplyOrder",
        ReadSupplyOrders => "ReadSupplyOrders",
        ActivateUser => "ActivateUser",
        ActivateUserGroup => "ActivateUserGroup",
        ActivateServiceAccount => "ActivateServiceAccount",
        _ => $"Action({code})",
    };
}
