using Fargo.Application.Common;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Audits;

internal static partial class AuditLogsQueryHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit logs query started for actor {actorGuid}. Page: {page}. Limit: {limit}.")]
    public static partial void ManyQueryStarted(
        this ILogger logger,
        Guid actorGuid, Page page, Limit limit);

    [LoggerMessage(
        Level = LogLevel.Debug,
        Message = "Audit logs query completed for actor {actorGuid}. EffectivePartitionCount: {effectivePartitionCount}. ResultCount: {resultCount}.")]
    public static partial void ManyQueryCompleted(
        this ILogger logger,
        Guid actorGuid,
        int effectivePartitionCount,
        int resultCount);
}
