using Fargo.Application.IntegrationEvents;
using Fargo.Core.Entities.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// Adds an <see cref="OutboxMessage"/> for each article event of a save, so the
/// events are written in the same database transaction as the change.
/// </summary>
/// <remarks>
/// <para>
/// Runs on every <c>SaveChanges</c>, whichever code path calls it, so a change
/// cannot be saved without its event. If the save fails, the messages are not
/// written either; they stay tracked as added, and a retried save does not add
/// them a second time.
/// </para>
/// <para>
/// Each message carries the partitions the article belonged to before or after
/// the change: the partition links stored in the database before the save, plus
/// every tracked link of the article (added, unchanged, modified or deleted).
/// Reading the stored links makes the result independent of whether the
/// caller loaded <see cref="Article.Partitions"/>.
/// </para>
/// </remarks>
public sealed class OutboxSaveChangesInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is { } context && DetectEvents(context) is { Count: > 0 } detected)
        {
            var stored = QueryStoredPartitions(context, detected).ToList();

            EnqueueEvents(context, detected, stored);
        }

        return result;
    }

    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is { } context && DetectEvents(context) is { Count: > 0 } detected)
        {
            var stored = await QueryStoredPartitions(context, detected).ToListAsync(cancellationToken);

            EnqueueEvents(context, detected, stored);
        }

        return result;
    }

    private static IReadOnlyList<(string EventType, Guid ArticleGuid)> DetectEvents(DbContext context)
    {
        // SaveChanges detects changes after this interceptor runs; detect them
        // now so the change tracker reflects every pending change.
        context.ChangeTracker.DetectChanges();

        return ArticleEventDetector.Detect(context.ChangeTracker);
    }

    private static IQueryable<PartitionLink> QueryStoredPartitions(
        DbContext context,
        IReadOnlyList<(string EventType, Guid ArticleGuid)> detected)
    {
        var articleGuids = detected.Select(d => d.ArticleGuid).Distinct().ToList();

        return context.Set<ArticlePartition>()
            .AsNoTracking()
            .Where(link => articleGuids.Contains(link.ArticleGuid))
            .Select(link => new PartitionLink(link.ArticleGuid, link.PartitionGuid));
    }

    private void EnqueueEvents(
        DbContext context,
        IReadOnlyList<(string EventType, Guid ArticleGuid)> detected,
        IReadOnlyList<PartitionLink> storedLinks)
    {
        var partitionsByArticle = CollectPartitions(context, detected, storedLinks);

        // Messages queued by an earlier, failed attempt of this save are still
        // tracked as added; do not queue the same event twice.
        var pending = context.ChangeTracker
            .Entries<OutboxMessage>()
            .Where(e => e.State == EntityState.Added)
            .Select(e => (e.Entity.EventType, e.Entity.AggregateGuid))
            .ToHashSet();

        var occurredAt = timeProvider.GetUtcNow();

        foreach (var (eventType, articleGuid) in detected)
        {
            if (!pending.Add((eventType, articleGuid)))
            {
                continue;
            }

            var message = new ArticleEventMessage(
                Guid.CreateVersion7(occurredAt),
                eventType,
                occurredAt,
                articleGuid,
                [.. partitionsByArticle[articleGuid].Order()]);

            context.Add(OutboxMessage.FromArticleEvent(message));
        }
    }

    private static Dictionary<Guid, HashSet<Guid>> CollectPartitions(
        DbContext context,
        IReadOnlyList<(string EventType, Guid ArticleGuid)> detected,
        IReadOnlyList<PartitionLink> storedLinks)
    {
        var partitionsByArticle = detected
            .Select(d => d.ArticleGuid)
            .Distinct()
            .ToDictionary(guid => guid, _ => new HashSet<Guid>());

        foreach (var link in storedLinks)
        {
            partitionsByArticle[link.ArticleGuid].Add(link.PartitionGuid);
        }

        // Tracked links cover partitions added in this save (not stored yet) and
        // removed ones (still stored, but listed here too for clarity).
        foreach (var entry in context.ChangeTracker.Entries<ArticlePartition>())
        {
            if (entry.State != EntityState.Detached &&
                partitionsByArticle.TryGetValue(entry.Entity.ArticleGuid, out var partitions))
            {
                partitions.Add(entry.Entity.PartitionGuid);
            }
        }

        return partitionsByArticle;
    }

    private sealed record PartitionLink(Guid ArticleGuid, Guid PartitionGuid);
}
