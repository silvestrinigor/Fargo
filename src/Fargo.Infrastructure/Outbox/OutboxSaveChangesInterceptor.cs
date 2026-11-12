using Fargo.Application.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// Adds an <see cref="OutboxMessage"/> for each article event of a save, so the
/// events are written in the same database transaction as the change.
/// </summary>
/// <remarks>
/// Runs on every <c>SaveChanges</c>, whichever code path calls it, so a change
/// cannot be saved without its event. If the save fails, the messages are not
/// written either; they stay tracked as added, and a retried save does not add
/// them a second time.
/// </remarks>
/// <param name="timeProvider">The clock used to stamp the events.</param>
public sealed class OutboxSaveChangesInterceptor(TimeProvider timeProvider) : SaveChangesInterceptor
{
    /// <inheritdoc />
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        EnqueueEvents(eventData.Context);

        return result;
    }

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        EnqueueEvents(eventData.Context);

        return ValueTask.FromResult(result);
    }

    private void EnqueueEvents(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        // SaveChanges detects changes after this interceptor runs; detect them
        // now so the change tracker reflects every pending change.
        context.ChangeTracker.DetectChanges();

        var detected = ArticleEventDetector.Detect(context.ChangeTracker);

        if (detected.Count == 0)
        {
            return;
        }

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
                articleGuid);

            context.Add(OutboxMessage.FromArticleEvent(message));
        }
    }
}
