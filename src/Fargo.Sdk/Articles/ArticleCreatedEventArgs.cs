namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides data for the <see cref="IArticleManager.Created"/> event.
/// </summary>
public sealed class ArticleCreatedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the created article.</summary>
    public Guid Guid { get; } = guid;
}
