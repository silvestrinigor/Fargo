namespace Fargo.Sdk.Articles;

/// <summary>
/// Provides data for the <see cref="IArticleManager.Deleted"/> event.
/// </summary>
public sealed class ArticleDeletedEventArgs(Guid guid) : EventArgs
{
    /// <summary>Gets the unique identifier of the deleted article.</summary>
    public Guid Guid { get; } = guid;
}
