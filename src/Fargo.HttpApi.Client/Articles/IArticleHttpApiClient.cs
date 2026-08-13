using Fargo.Application;
using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;
using Fargo.HttpApi.Client.Common;

namespace Fargo.HttpApi.Client.Articles;

public interface IArticleHttpApiClient
{
    Task<FargoHttpResponse<ArticleDto>> GetByGuidAsync(Guid articleGuid, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse<ArticleDto>> GetByBarcodeAsync(Barcode barcode, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse<IReadOnlyCollection<ArticleDto>>> GetManyAsync(Pagination? pagination = null, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse<Guid>> CreateAsync(ArticleCreateDto request, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse> PatchAsync(Guid articleGuid, ArticleUpdateDto request, CancellationToken cancellationToken = default);

    Task<FargoHttpResponse> DeleteAsync(Guid articleGuid, CancellationToken cancellationToken = default);
}
