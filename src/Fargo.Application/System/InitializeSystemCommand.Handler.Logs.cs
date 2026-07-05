using Microsoft.Extensions.Logging;

namespace Fargo.Application.System;

internal static partial class InitializeSystemCommandHandlerLogs
{
    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System initialization flow started.")]
    public static partial void InitializeSystemStarted(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System initialization flow skipped because users already exist.")]
    public static partial void InitializeSystemSkiped(this ILogger logger);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "System initialization flow completed. Global partition created: {globalPartitionCreated}. Administrators group created: {administratorsUserGroupCreated}. Admin user created: {adminUserCreated}")]
    public static partial void InitializeSystemCompleted(
        this ILogger logger,
        bool globalPartitionCreated,
        bool administratorsUserGroupCreated,
        bool adminUserCreated);
}
