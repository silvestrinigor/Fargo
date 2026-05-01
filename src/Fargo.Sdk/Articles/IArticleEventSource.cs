namespace Fargo.Api.Articles;

/// <summary>Broadcasts the hub <c>OnArticleCreated</c> event as a typed .NET event.</summary>
public interface IArticleEventSource
{
    /// <summary>Raised when any authenticated client creates an article.</summary>
    event EventHandler<ArticleCreatedEventArgs>? Created;
}
