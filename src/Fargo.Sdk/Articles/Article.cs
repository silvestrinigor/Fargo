using Fargo.Sdk.Partitions;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Represents a live article entity. Call <see cref="UpdateAsync"/> to persist property changes.
/// </summary>
public sealed class Article
{
    internal Article(Guid guid, string name, string description, IArticleClient client)
    {
        Guid = guid;
        _name = name;
        _description = description;
        this.client = client;
    }

    private readonly IArticleClient client;

    /// <summary>The unique identifier of the article.</summary>
    public Guid Guid { get; }

    private string _name;

    /// <summary>The display name of the article.</summary>
    public string Name
    {
        get => _name;
        set => _name = value;
    }

    private string _description;

    /// <summary>The description of the article.</summary>
    public string Description
    {
        get => _description;
        set => _description = value;
    }

    /// <summary>Raised when this article is updated by any authenticated client.</summary>
    public event EventHandler<ArticleUpdatedEventArgs>? Updated;

    /// <summary>Raised when this article is deleted by any authenticated client.</summary>
    public event EventHandler<ArticleDeletedEventArgs>? Deleted;

    internal void RaiseUpdated() => Updated?.Invoke(this, new ArticleUpdatedEventArgs(Guid));

    internal void RaiseDeleted() => Deleted?.Invoke(this, new ArticleDeletedEventArgs(Guid));

    /// <summary>
    /// Gets the partitions that directly contain this article.
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    public Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        CancellationToken cancellationToken = default)
        => client.GetPartitionsAsync(Guid, cancellationToken);

    /// <summary>
    /// Applies <paramref name="update"/> to this article and persists all changes in a single request.
    /// </summary>
    /// <param name="update">An action that sets one or more properties on this article.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <exception cref="FargoSdkApiException">Thrown if the update fails.</exception>
    public async Task UpdateAsync(Action<Article> update, CancellationToken cancellationToken = default)
    {
        update(this);
        var result = await client.UpdateAsync(Guid, _name, _description, cancellationToken);
        if (!result.IsSuccess)
        {
            throw new FargoSdkApiException(result.Error!.Detail);
        }
    }
}
