namespace Fargo.Api.Articles;

/// <summary>Default implementation of <see cref="IArticleBarcodeService"/>.</summary>
public sealed class ArticleBarcodeService : IArticleBarcodeService
{
    /// <summary>Initializes a new instance.</summary>
    public ArticleBarcodeService(IArticleHttpClient client)
    {
        this.client = client;
    }

    private readonly IArticleHttpClient client;

    /// <inheritdoc />
    public async Task<ArticleBarcodes> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.GetBarcodesAsync(articleGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new FargoSdkApiException(response.Error!);
        }

        return response.Data ?? new ArticleBarcodes();
    }

    /// <inheritdoc />
    public async Task UpdateBarcodesAsync(
        Guid articleGuid,
        ArticleBarcodes barcodes,
        CancellationToken cancellationToken = default)
    {
        var response = await client.UpdateBarcodesAsync(articleGuid, barcodes, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new FargoSdkApiException(response.Error!);
        }
    }
}
