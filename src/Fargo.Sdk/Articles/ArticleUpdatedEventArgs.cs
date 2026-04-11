namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides data for the <see cref="IArticleManager.Updated"/> event.
/// </summary>
public sealed class ArticleUpdatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the updated article.</summary>
    public Guid Guid { get; } = guid;
}
