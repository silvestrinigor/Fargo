namespace Fargo.WorkflowTest;

/// <summary>
/// The permission action codes as exposed by the API. These mirror
/// <c>Fargo.Core.Actors.ActionType</c>; the generated client transmits them as
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

    /// <summary>Returns a human-readable name for a permission code.</summary>
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
        _ => $"Action({code})",
    };
}
