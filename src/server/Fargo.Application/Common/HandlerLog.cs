using Fargo.Core.Actors;
using Microsoft.Extensions.Logging;

namespace Fargo.Application.Common;

/// <summary>
/// Shared, source-generated logging for application handlers.
/// <para>
/// Actor identity (<c>ActorGuid</c> / <c>ActorType</c>) is attached once per
/// handler invocation through <see cref="BeginActorScope"/> rather than
/// repeated on every log call. All messages emitted inside that scope —
/// including lower-layer logs — automatically carry the actor fields in
/// structured sinks.
/// </para>
/// </summary>
public static partial class HandlerLog
{
    // ── Actor scope ─────────────────────────────────────────────────

    /// <summary>
    /// Opens a logging scope that stamps the current actor's identity onto
    /// every log entry emitted while the scope is active.
    /// </summary>
    public static IDisposable? BeginActorScope(this ILogger logger, ICurrentActor actor)
        => logger.BeginScope(new Dictionary<string, object>
        {
            ["ActorGuid"] = actor.Guid,
            ["ActorType"] = actor.ActorType,
        });

    // ── Command operations (Information) ────────────────────────────

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{Operation} started.")]
    public static partial void CommandStarted(this ILogger logger, string operation);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{Operation} started for {EntityType} '{EntityGuid}'.")]
    public static partial void CommandStarted(this ILogger logger, string operation, string entityType, Guid entityGuid);

    [LoggerMessage(Level = LogLevel.Information,
        Message = "{Operation} completed for {EntityType} '{EntityGuid}'.")]
    public static partial void CommandCompleted(this ILogger logger, string operation, string entityType, Guid entityGuid);

    // ── Query lookups (Debug) ───────────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} started for {EntityType} '{EntityGuid}'.")]
    public static partial void QueryStarted(this ILogger logger, string operation, string entityType, Guid entityGuid);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} started for {EntityType} '{EntityKey}'.")]
    public static partial void QueryStarted(this ILogger logger, string operation, string entityType, string entityKey);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} completed for {EntityType} '{EntityGuid}'. Found: {Found}.")]
    public static partial void QueryCompleted(this ILogger logger, string operation, string entityType, Guid entityGuid, bool found);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} completed for {EntityType} '{EntityKey}'. Found: {Found}.")]
    public static partial void QueryCompleted(this ILogger logger, string operation, string entityType, string entityKey, bool found);

    // ── Collection queries (Debug) ──────────────────────────────────

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} started. Page: {Page}. Limit: {Limit}.")]
    public static partial void CollectionStarted(this ILogger logger, string operation, int page, int limit);

    [LoggerMessage(Level = LogLevel.Debug,
        Message = "{Operation} completed. ResultCount: {ResultCount}.")]
    public static partial void CollectionCompleted(this ILogger logger, string operation, int resultCount);
}
