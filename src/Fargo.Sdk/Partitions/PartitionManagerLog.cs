using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Partitions;

internal static class PartitionManagerLog
{
    private static readonly Action<ILogger, Guid, string, Exception?> _partitionUpdateFailed =
        LoggerMessage.Define<Guid, string>(LogLevel.Error, default, "Failed to update partition {PartitionGuid}: {Detail}");

    internal static void LogPartitionUpdateFailed(this ILogger logger, Guid partitionGuid, string detail)
        => _partitionUpdateFailed(logger, partitionGuid, detail, null);
}
