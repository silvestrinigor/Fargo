namespace Fargo.Core.Audits;

/// <summary>
/// Coarse family of a command; the exact command is in <see cref="AuditLog.CommandName"/>.
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
}
