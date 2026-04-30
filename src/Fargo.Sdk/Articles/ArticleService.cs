using Fargo.Sdk.Events;
using System.Collections.Concurrent;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Default implementation of <see cref="IArticleService"/>. Tracks live entities and routes
/// hub Updated/Deleted events to them.
/// </summary>
public sealed class ArticleService : IArticleService
{
    /// <summary>Initializes a new instance.</summary>
    public ArticleService(IArticleHttpClient client, IFargoEventHub hub)
    {
        this.client = client;

        hub.On<Guid>("OnArticleUpdated", guid =>
        {
            if (tracked.TryGetValue(guid, out var article))
            {
                article.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnArticleDeleted", guid =>
        {
            if (tracked.TryGetValue(guid, out var article))
            {
                article.RaiseDeleted();
            }
        });

        this.hub = hub;
    }

    private readonly ConcurrentDictionary<Guid, Article> tracked = new();
    private readonly IArticleHttpClient client;
    private readonly IFargoEventHub hub;

    /// <inheritdoc />
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

        return await ToEntityAsync(response.Data!);
    }

    /// <inheritdoc />
    public async Task<IReadOnlyCollection<Article>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetManyAsync(temporalAsOf, page, limit, partitionGuid, search, noPartition, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var entities = new List<Article>();
        foreach (var r in response.Data!)
        {
            entities.Add(await ToEntityAsync(r));
        }

        return entities;
    }

    /// <inheritdoc />
    public async Task<Article> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, firstPartition, metrics, shelfLife, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var article = new Article(response.Data, name, description ?? string.Empty, client, MakeDisposeCallback(response.Data), metrics, shelfLife);
        return await TrackAsync(article);
    }

    /// <inheritdoc />
    public async Task DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<Article> ToEntityAsync(ArticleResult r)
    {
        var article = new Article(r.Guid, r.Name, r.Description, client, MakeDisposeCallback(r.Guid), r.Metrics, r.ShelfLife, r.Images, r.EditedByGuid);
        return await TrackAsync(article);
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.TryRemove(guid, out _);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private async Task<Article> TrackAsync(Article article)
    {
        var trackedArticle = tracked.GetOrAdd(article.Guid, article);

        if (ReferenceEquals(trackedArticle, article))
        {
            await hub.InvokeAsync("SubscribeToEntityAsync", article.Guid);
        }

        return trackedArticle;
    }

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error);
}
