using Fargo.Sdk.Events;

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

    private readonly Dictionary<Guid, Article> tracked = new();
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
        MassDto? mass = null,
        LengthDto? lengthX = null,
        LengthDto? lengthY = null,
        LengthDto? lengthZ = null,
        CancellationToken cancellationToken = default)
    {
        var response = await client.CreateAsync(name, description, firstPartition, mass, lengthX, lengthY, lengthZ, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        var article = new Article(response.Data, name, description ?? string.Empty, mass, client, MakeDisposeCallback(response.Data), lengthX, lengthY, lengthZ, hasImage: false);
        tracked[article.Guid] = article;
        await hub.InvokeAsync("SubscribeToEntityAsync", article.Guid);
        return article;
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
        var article = new Article(r.Guid, r.Name, r.Description, r.Mass, client, MakeDisposeCallback(r.Guid), r.LengthX, r.LengthY, r.LengthZ, r.HasImage, r.EditedByGuid);
        tracked[article.Guid] = article;
        await hub.InvokeAsync("SubscribeToEntityAsync", article.Guid);
        return article;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
