namespace Fargo.Core.Audits;

/// <summary>
/// Represents the kind of command an audit log records.
/// </summary>
/// <remarks>
/// An audit log describes the command an actor executed. These values classify
/// that command into a coarse action family; the exact command is additionally
/// captured by the audit log's command name, and the command's data is captured
/// in its metadata.
/// </remarks>
public enum AuditLogType
{
    /// <summary>
    /// A command that created an entity.
    /// </summary>
    EntityCreated = 1,

    /// <summary>
    /// A command that updated an entity.
    /// </summary>
    EntityUpdated = 2,

    /// <summary>
    /// A command that deleted an entity.
    /// </summary>
    EntityDeleted = 3,

    /// <summary>
    /// A command that recorded an event against an event-tracked entity, such as
    /// a movement of a quantity of an event-tracked article.
    /// </summary>
    ItemEventRecorded = 4,

    /// <summary>
    /// A command that authenticated an actor (login).
    /// </summary>
    Authenticated = 5,

    /// <summary>
    /// A command that logged an actor out.
    /// </summary>
    LoggedOut = 6,

    /// <summary>
    /// A command that refreshed an authentication token.
    /// </summary>
    TokenRefreshed = 7,

    /// <summary>
    /// A command that issued a service-account access token.
    /// </summary>
    ServiceTokenIssued = 8,

    /// <summary>
    /// A command that changed an actor's password.
    /// </summary>
    PasswordChanged = 9,

    /// <summary>
    /// A command that rotated a service account's secret.
    /// </summary>
    ServiceAccountSecretRotated = 10,

    /// <summary>
    /// A command that initialized the system.
    /// </summary>
    SystemInitialized = 11,

    /// <summary>
    /// A command that placed a supply order.
    /// </summary>
    SupplyOrderPlaced = 12,

    /// <summary>
    /// A command that received goods against a supply order.
    /// </summary>
    SupplyOrderReceived = 13,

    /// <summary>
    /// A command that cancelled a supply order.
    /// </summary>
    SupplyOrderCancelled = 14,

    /// <summary>
    /// A command that closed a supply order.
    /// </summary>
    SupplyOrderClosed = 15,
}
