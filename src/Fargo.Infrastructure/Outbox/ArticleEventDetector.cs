using Fargo.Application.IntegrationEvents;
using Fargo.Core.Entities.Articles;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Fargo.Infrastructure.Outbox;

/// <summary>
/// Works out which article events a pending save produces, from the state of the
/// tracked entities.
/// </summary>
/// <remarks>
/// <list type="bullet">
///   <item><description>An added <see cref="Article"/> produces <see cref="ArticleEventTypes.Created"/>.</description></item>
///   <item><description>A deleted <see cref="Article"/> produces <see cref="ArticleEventTypes.Deleted"/>.</description></item>
///   <item><description>
///     A modified <see cref="Article"/>, or an added, modified, or deleted part of
///     an article (barcode, dimension, variation, pack, kit component, container,
///     container allowed article, or partition link) produces
///     <see cref="ArticleEventTypes.Edited"/> for the owning article, unless that
///     article is itself being created or deleted in the same save.
///   </description></item>
/// </list>
/// Each article produces at most one event per save. Changes made outside the
/// change tracker (raw SQL, <c>ExecuteUpdate</c>/<c>ExecuteDelete</c>, or
/// database-level cascades of untracked rows) are not seen.
/// </remarks>
public static class ArticleEventDetector
{
    /// <summary>
    /// Detects the article events of the pending changes.
    /// </summary>
    /// <param name="changeTracker">
    /// The change tracker, on which change detection must already have run.
    /// </param>
    /// <returns>
    /// The event type and article identifier of each event, in the order the
    /// articles were first seen in the change tracker.
    /// </returns>
    public static IReadOnlyList<(string EventType, Guid ArticleGuid)> Detect(ChangeTracker changeTracker)
    {
        ArgumentNullException.ThrowIfNull(changeTracker);

        var order = new List<Guid>();
        var rootEvents = new Dictionary<Guid, string>();
        var editedParts = new HashSet<Guid>();

        foreach (var entry in changeTracker.Entries())
        {
            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (entry.Entity is Article article)
            {
                Remember(order, article.Guid);

                rootEvents[article.Guid] = entry.State switch
                {
                    EntityState.Added => ArticleEventTypes.Created,
                    EntityState.Deleted => ArticleEventTypes.Deleted,
                    _ => ArticleEventTypes.Edited,
                };

                continue;
            }

            if (GetOwningArticleGuid(entry) is { } ownerGuid)
            {
                Remember(order, ownerGuid);

                editedParts.Add(ownerGuid);
            }
        }

        var events = new List<(string, Guid)>(order.Count);

        foreach (var articleGuid in order)
        {
            // A created or deleted article reports only that, even when its
            // parts changed with it; otherwise a part change is an edit.
            if (rootEvents.TryGetValue(articleGuid, out var eventType))
            {
                events.Add((eventType, articleGuid));
            }
            else if (editedParts.Contains(articleGuid))
            {
                events.Add((ArticleEventTypes.Edited, articleGuid));
            }
        }

        return events;
    }

    private static Guid? GetOwningArticleGuid(EntityEntry entry) => entry.Entity switch
    {
        ArticleBarcode barcode => barcode.ArticleGuid,
        ArticleDimension dimension => dimension.ArticleGuid,
        ArticleVariation variation => variation.VariationArticleGuid,
        ArticlePack pack => pack.PackArticleGuid,
        ArticleKitComponent kitComponent => kitComponent.KitArticleGuid,
        ArticleContainer container => container.ArticleGuid,
        ArticleContainerAllowedArticle allowedArticle => allowedArticle.ContainerArticleGuid,
        ArticlePartition partition => partition.ArticleGuid,
        _ => null,
    };

    private static void Remember(List<Guid> order, Guid articleGuid)
    {
        if (!order.Contains(articleGuid))
        {
            order.Add(articleGuid);
        }
    }
}
