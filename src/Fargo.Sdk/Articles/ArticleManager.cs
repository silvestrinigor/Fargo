using Fargo.Sdk.Events;
using Microsoft.Extensions.Logging;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Default implementation of <see cref="IArticleManager"/>.
/// </summary>
public sealed class ArticleManager : IArticleManager
{
    internal ArticleManager(IArticleClient client, FargoHubConnection hub, ILogger logger)
    {
        this.client = client;
        this.logger = logger;

        hub.On<Guid>("OnArticleCreated", guid =>
            Created?.Invoke(this, new ArticleCreatedEventArgs(guid)));

        hub.On<Guid>("OnArticleUpdated", guid =>
            Updated?.Invoke(this, new ArticleUpdatedEventArgs(guid)));

        hub.On<Guid>("OnArticleDeleted", guid =>
            Deleted?.Invoke(this, new ArticleDeletedEventArgs(guid)));
    }

    public event EventHandler<ArticleCreatedEventArgs>? Created;
    public event EventHandler<ArticleUpdatedEventArgs>? Updated;
    public event EventHandler<ArticleDeletedEventArgs>? Deleted;

    private readonly IArticleClient client;
    private readonly ILogger logger;

    public async Task<Article> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetAsync(articleGuid, temporalAsOf, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return ToEntity(response.Data!);
    }

    public async Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(temporalAsOf, page, limit, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data!.Select(ToEntity).ToList();
    }

    public async Task<Article> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, firstPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return new Article(response.Data, name, description ?? string.Empty, client, logger);
    }

    public async Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private Article ToEntity(ArticleResult r) => new(r.Guid, r.Name, r.Description, client, logger);

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
