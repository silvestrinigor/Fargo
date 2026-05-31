using Fargo.Application.Shared.Articles;
using Fargo.Core.Shared.Barcodes;

namespace Fargo.HttpClient;

public interface IFargoArticleClient
{
    Task<ArticleDto?> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<ArticleDto?> GetByBarcodeAsync(
        string code,
        BarcodeFormat format,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<ArticleDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(
        ArticleCreateDto request,
        CancellationToken cancellationToken = default);

    Task PatchAsync(
        Guid articleGuid,
        ArticlePatchDto request,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default);
}

internal sealed class FargoArticleClient(FargoHttpTransport transport) : IFargoArticleClient
{
    public Task<ArticleDto?> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<ArticleDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/articles/{articleGuid:D}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<ArticleDto?> GetByBarcodeAsync(
        string code,
        BarcodeFormat format,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
        => transport.SendNullableAsync<ArticleDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath(
                $"/articles/{Uri.EscapeDataString($"{code}:{format}")}",
                FargoHttpTransport.SingleQuery(temporalAsOf)),
            null,
            cancellationToken);

    public Task<IReadOnlyCollection<ArticleDto>> GetManyAsync(
        FargoListQuery? query = null,
        CancellationToken cancellationToken = default)
        => transport.SendCollectionAsync<ArticleDto>(
            HttpMethod.Get,
            FargoHttpTransport.BuildPath("/articles/", FargoHttpTransport.ListQuery(query)),
            null,
            cancellationToken);

    public Task<Guid> CreateAsync(
        ArticleCreateDto request,
        CancellationToken cancellationToken = default)
        => transport.SendRequiredAsync<Guid>(
            HttpMethod.Post,
            "/articles/",
            request,
            cancellationToken);

    public Task PatchAsync(
        Guid articleGuid,
        ArticlePatchDto request,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Patch,
            $"/articles/{articleGuid:D}",
            request,
            cancellationToken);

    public Task DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => transport.SendNoContentAsync(
            HttpMethod.Delete,
            $"/articles/{articleGuid:D}",
            null,
            cancellationToken);
}
