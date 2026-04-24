using Fargo.Sdk.Events;

namespace Fargo.Sdk.Articles;

/// <summary>
/// Default implementation of <see cref="IArticleManager"/>.
/// </summary>
public sealed class ArticleManager : IArticleManager
{
    internal ArticleManager(IArticleClient client, FargoHubConnection hub)
    {
        this.client = client;
        this.hub = hub;

        hub.On<Guid>("OnArticleCreated", guid =>
            Created?.Invoke(this, new ArticleCreatedEventArgs(guid)));

        hub.On<Guid>("OnArticleUpdated", guid =>
        {
            if (_tracked.TryGetValue(guid, out var article))
            {
                article.RaiseUpdated();
            }
        });

        hub.On<Guid>("OnArticleDeleted", guid =>
        {
            if (_tracked.TryGetValue(guid, out var article))
            {
                article.RaiseDeleted();
            }
        });
    }

    public event EventHandler<ArticleCreatedEventArgs>? Created;

    private readonly Dictionary<Guid, Article> _tracked = new();
    private readonly IArticleClient client;
    private readonly FargoHubConnection hub;

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
        _tracked[article.Guid] = article;
        await hub.InvokeAsync("SubscribeToEntityAsync", article.Guid);
        return article;
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

    public async Task UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default)
    {
        var response = await client.UploadImageAsync(articleGuid, stream, contentType, fileName, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        if (_tracked.TryGetValue(articleGuid, out var article))
        {
            article.HasImage = true;
        }
    }

    public async Task DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteImageAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        if (_tracked.TryGetValue(articleGuid, out var article))
        {
            article.HasImage = false;
        }
    }

    public Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => client.GetImageAsync(articleGuid, cancellationToken);

    public async Task<IReadOnlyCollection<BarcodeResult>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetBarcodesAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data ?? [];
    }

    public async Task<Guid> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default)
    {
        var response = await client.AddBarcodeAsync(articleGuid, code, format, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }

        return response.Data;
    }

    public async Task RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.RemoveBarcodeAsync(articleGuid, barcodeGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            ThrowError(response.Error!);
        }
    }

    private async Task<Article> ToEntityAsync(ArticleResult r)
    {
        var article = new Article(r.Guid, r.Name, r.Description, r.Mass, client, MakeDisposeCallback(r.Guid), r.LengthX, r.LengthY, r.LengthZ, r.HasImage, r.EditedByGuid);
        _tracked[article.Guid] = article;
        await hub.InvokeAsync("SubscribeToEntityAsync", article.Guid);
        return article;
    }

    private Func<ValueTask> MakeDisposeCallback(Guid guid) => async () =>
    {
        _tracked.Remove(guid);
        await hub.InvokeAsync("UnsubscribeFromEntityAsync", guid);
    };

    private static void ThrowError(FargoSdkError error) =>
        throw new FargoSdkApiException(error.Detail);
}
