namespace Fargo.Api.Articles;

/// <summary>Default implementation of <see cref="IArticleImageService"/>.</summary>
public sealed class ArticleImageService : IArticleImageService
{
    /// <summary>Initializes a new instance.</summary>
    public ArticleImageService(IArticleHttpClient client)
    {
        this.client = client;
    }

    private readonly IArticleHttpClient client;

    /// <inheritdoc />
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
            throw new FargoSdkApiException(response.Error!);
        }
    }

    /// <inheritdoc />
    public async Task DeleteImageAsync(Guid articleGuid, CancellationToken cancellationToken = default)
    {
        var response = await client.DeleteImageAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new FargoSdkApiException(response.Error!);
        }
    }

    /// <inheritdoc />
    public Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => client.GetImageAsync(articleGuid, cancellationToken);
}
