using Fargo.Application;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.HttpApi.Client.Common;

namespace Fargo.HttpApi.Client.Articles;

public sealed class ArticleHttpApiClient(FargoHttpClient fargoHttpClient) : IArticleHttpApiClient
{
    public Task<FargoHttpResponse<Guid>> CreateAsync(ArticleCreateDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PostAsync<ArticleCreateDto, Guid>($"articles", request, cancellationToken);

    public Task<FargoHttpResponse> DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => fargoHttpClient.DeleteAsync($"articles/{articleGuid:D}", cancellationToken);

    public Task<FargoHttpResponse<ArticleDto>> GetByBarcodeAsync(Barcode barcode, CancellationToken cancellationToken = default)
        => fargoHttpClient.GetAsync<ArticleDto>($"articles/{barcode}", cancellationToken);

    public Task<FargoHttpResponse<ArticleDto>> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default)
        => fargoHttpClient.GetAsync<ArticleDto>($"articles/{articleGuid:D}", cancellationToken);

    public Task<FargoHttpResponse<IReadOnlyCollection<ArticleDto>>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default)
        => fargoHttpClient.GetCollectionAsync<ArticleDto>($"articles", cancellationToken);

    public Task<FargoHttpResponse> PatchAsync(Guid articleGuid, ArticleUpdateDto request, CancellationToken cancellationToken = default)
        => fargoHttpClient.PatchAsync($"articles/{articleGuid:D}", request, cancellationToken);
}
