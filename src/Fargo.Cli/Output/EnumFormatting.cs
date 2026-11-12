using System.Text;

namespace Fargo.Cli.Output;

/// <summary>
/// Mirrors the server's ArticleType enum.
/// </summary>
public enum ArticleType
{
    Default = 1,
    Variation = 2,
    Pack = 3,
    Kit = 4,
    Container = 5,
}

/// <summary>
/// Mirrors the server's ActorType enum.
/// </summary>
public enum ActorType
{
    User = 1,
    Service = 2,
}

/// <summary>
/// Mirrors the server's AuditLogType enum.
/// </summary>
public enum AuditLogType
{
    EntityCreated = 1,
    EntityUpdated = 2,
    EntityDeleted = 3,
    ItemEventRecorded = 4,
    Authenticated = 5,
    LoggedOut = 6,
    TokenRefreshed = 7,
    ServiceTokenIssued = 8,
    PasswordChanged = 9,
    ServiceAccountSecretRotated = 10,
    SystemInitialized = 11,
    SupplyOrderPlaced = 12,
    SupplyOrderReceived = 13,
    SupplyOrderCancelled = 14,
    SupplyOrderClosed = 15,
    PermissionsChanged = 16,
}

/// <summary>
/// Mirrors the server's EntityType enum.
/// </summary>
public enum EntityType
{
    Article = 0,
    Item = 1,
    Actor = 2,
    UserGroup = 3,
    Partition = 4,
    AuditLog = 5,
    ItemEvent = 6,
}

/// <summary>
/// Mirrors the server's ActionType enum.
/// </summary>
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
    EditServiceAccount = 22,
    ReadServiceAccount = 23,
    CreateSupplyOrder = 24,
    EditSupplyOrder = 25,
    PlaceSupplyOrder = 26,
    ReceiveSupplyOrder = 27,
    CancelSupplyOrder = 28,
    CloseSupplyOrder = 29,
    ReadSupplyOrders = 30,
    ActivateUser = 31,
    ActivateUserGroup = 32,
    ActivateServiceAccount = 33,
    MoveItem = 34,
    ChangeUserPermissions = 35,
    ChangeUserGroupPermissions = 36,
    ChangeServiceAccountPermissions = 37,
}

/// <summary>
/// Mirrors the server's ItemEventType enum.
/// </summary>
public enum ItemEventType
{
    Moviment = 1,
    Creation = 2,
    Deletion = 3,
}

/// <summary>
/// Mirrors the server's ArticleTrackingMode enum.
/// </summary>
public enum ArticleTrackingMode
{
    Entity = 1,
    Event = 2,
}

/// <summary>
/// Formats enum values as kebab-case names for display.
/// </summary>
public static class EnumFormatting
{
    /// <summary>
    /// Returns <see langword="null"/> for a null code and "unknown (N)" for a code the enum does not define.
    /// </summary>
    public static string? Format<TEnum>(int? code) where TEnum : struct, Enum
    {
        if (code is null)
        {
            return null;
        }

        if (Enum.IsDefined(typeof(TEnum), code.Value))
        {
            var name = Enum.GetName(typeof(TEnum), code.Value)!;
            return ToKebabCase(name);
        }

        return $"unknown ({code.Value})";
    }

    public static string Format<TEnum>(TEnum value) where TEnum : struct, Enum
    {
        return ToKebabCase(value.ToString());
    }

    public static string ToKebabCase(string name)
    {
        var builder = new StringBuilder(name.Length + 8);

        for (var i = 0; i < name.Length; i++)
        {
            var c = name[i];

            if (char.IsUpper(c))
            {
                if (i > 0)
                {
                    builder.Append('-');
                }

                builder.Append(char.ToLowerInvariant(c));
            }
            else
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }
}
