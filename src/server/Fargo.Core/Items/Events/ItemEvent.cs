using Fargo.Core.Articles;
using Fargo.Core.Entities;

namespace Fargo.Core.Items.Events;

// TODO: Doc
public class ItemEvent : IEntity
{
    public Guid Guid { get; private init; } = Guid.NewGuid();

    public Guid ArticleGuid { get; private init; }

    public Article Article { get; private init; }

    public ItemEventType EventType { get; private init; }

    public DateTimeOffset OccuredAt { get; private init; } = DateTimeOffset.UtcNow;

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
    private ItemEvent() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

    internal ItemEvent(Article article, ItemEventType eventType)
    {
        // TODO: If article is not a item event type, throw a exception.

        ArticleGuid = article.Guid;
        Article = article;

        EventType = eventType;
    }
}
