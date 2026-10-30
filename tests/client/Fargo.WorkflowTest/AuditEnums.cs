namespace Fargo.WorkflowTest;

/// <summary>
/// The actor-type codes as exposed by the API. Mirrors
/// <c>Fargo.Core.Actors.ActorType</c>; the generated client transmits them as
/// plain integers.
/// </summary>
internal static class ActorTypes
{
    public const int User = 1;

    public static string Name(int? code) => code switch
    {
        User => "User",
        null => "(none)",
        _ => $"ActorType({code})",
    };
}

/// <summary>
/// The entity-type codes as exposed by the API. Mirrors
/// <c>Fargo.Core.Entities.EntityType</c>.
/// </summary>
internal static class EntityTypes
{
    public const int Article = 0;
    public const int Item = 1;
    public const int User = 2;
    public const int UserGroup = 3;
    public const int Partition = 4;
    public const int AuditLog = 5;
    public const int ItemEvent = 6;

    public static string Name(int? code) => code switch
    {
        Article => "Article",
        Item => "Item",
        User => "User",
        UserGroup => "UserGroup",
        Partition => "Partition",
        AuditLog => "AuditLog",
        ItemEvent => "ItemEvent",
        null => "(none)",
        _ => $"EntityType({code})",
    };
}

/// <summary>
/// The audit-log-type codes as exposed by the API. Mirrors
/// <c>Fargo.Core</c>'s <c>AuditLogType</c> enum.
/// </summary>
internal static class AuditLogTypes
{
    public const int EntityCreated = 1;
    public const int EntityUpdated = 2;
    public const int EntityDeleted = 3;
    public const int ItemEventRecorded = 4;

    public static string Name(int? code) => code switch
    {
        EntityCreated => "EntityCreated",
        EntityUpdated => "EntityUpdated",
        EntityDeleted => "EntityDeleted",
        ItemEventRecorded => "ItemEventRecorded",
        null => "(none)",
        _ => $"AuditLogType({code})",
    };
}
