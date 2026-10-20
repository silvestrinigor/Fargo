using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

internal static partial class AuditLogSingleQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit log single query started for audit log '{auditLogGuid}' by actor '{actorGuid}' of type '{actorType}'.")]
    public static partial void AuditLogSingleQueryStarted(
        this ILogger logger, Guid auditLogGuid, Guid actorGuid, ActorType actorType);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit log single query completed for audit log '{auditLogGuid}' by actor '{actorGuid}'. Found: {found}.")]
    public static partial void AuditLogSingleQueryCompleted(
        this ILogger logger, Guid auditLogGuid, Guid actorGuid, bool found);
}
