using Fargo.Sdk.Events;

namespace Fargo.Sdk.Articles;

/// <summary>Default implementation of <see cref="IArticleEventSource"/>.</summary>
public sealed class ArticleEventSource : IArticleEventSource
{
    /// <summary>Initializes a new instance.</summary>
    public ArticleEventSource(IFargoEventHub hub)
    {
        hub.On<Guid>("OnArticleCreated", guid =>
            Created?.Invoke(this, new ArticleCreatedEventArgs(guid)));
    }

    /// <inheritdoc />
    public event EventHandler<ArticleCreatedEventArgs>? Created;
}
