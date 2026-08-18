using Fargo.Application.Common;
using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

internal static partial class AuditLogsQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit logs query started for actor '{actorGuid}' of type '{actorType}'. Page: {page}. Limit: {limit}.")]
    public static partial void ManyQueryStarted(
        this ILogger logger,
        Guid actorGuid, ActorType actorType, Page page, Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit logs query completed for actor '{actorGuid}' of type '{actorType}'. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ManyQueryCompleted(
        this ILogger logger,
        Guid actorGuid,
        ActorType actorType,
        int effectivePartitionCount,
        int resultCount);
}
