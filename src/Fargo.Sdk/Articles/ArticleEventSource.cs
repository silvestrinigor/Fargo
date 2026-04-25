using Fargo.Sdk.Events;

namespace Fargo.Sdk.Articles;

/// <summary>Default implementation of <see cref="IArticleEventSource"/>.</summary>
public sealed class ArticleEventSource : IArticleEventSource
{
    public ArticleEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnArticleCreated", guid =>
            Created?.Invoke(this, new ArticleCreatedEventArgs(guid)));
    }

    public event EventHandler<ArticleCreatedEventArgs>? Created;
}
