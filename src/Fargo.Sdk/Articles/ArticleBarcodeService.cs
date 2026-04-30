namespace Fargo.Sdk.Articles;

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
    public async Task<Guid> AddBarcodeAsync(
        Guid articleGuid,
        string code,
        BarcodeFormat format,
        CancellationToken cancellationToken = default)
    {
        var response = await client.AddBarcodeAsync(articleGuid, code, format, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new FargoSdkApiException(response.Error!);
        }

        return response.Data;
    }

    /// <inheritdoc />
    public async Task RemoveBarcodeAsync(
        Guid articleGuid,
        Guid barcodeGuid,
        CancellationToken cancellationToken = default)
    {
        var response = await client.RemoveBarcodeAsync(articleGuid, barcodeGuid, cancellationToken);

        if (!response.IsSuccess)
        {
            throw new FargoSdkApiException(response.Error!);
        }
    }
}
