using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Articles;

/// <summary>Default implementation of <see cref="IArticleClient"/>.</summary>
public sealed class ArticleHttpClient : IArticleClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ArticleHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoResponse<ArticleInfo>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ArticleInfo>($"/articles/{articleGuid}{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<ArticleInfo>> GetByBarcodeAsync(
        ArticleBarcode articleBarcode,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var pathBarcode = Uri.EscapeDataString(articleBarcode.Barcode);
        var httpResponse = await httpClient.GetAsync<ArticleInfo>(
            $"/articles/{pathBarcode}:{articleBarcode.Type}{query}",
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse<IReadOnlyCollection<ArticleInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? childOfAnyOfThesePartitions = null,
        bool? notChildOfAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<(string Key, string? Value)>
        {
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("notChildOfAnyPartition", notChildOfAnyPartition?.ToString())
        };
        parameters.AddRange(childOfAnyOfThesePartitions?.Select(partitionGuid =>
            ("childOfAnyOfThesePartitions", (string?)partitionGuid.ToString())) ?? []);
        var query = FargoHttpClient.BuildQuery([.. parameters]);

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ArticleInfo>>($"/articles{query}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse, Array.Empty<ArticleInfo>());
    }

    /// <inheritdoc />
    public async Task<FargoResponse<Guid>> CreateAsync(
        ArticleCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ArticleCreateRequest, Guid>(
            "/articles",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> UpdateAsync(
        Guid articleGuid,
        ArticleUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<ArticleUpdateRequest>(
            $"/articles/{articleGuid}",
            request,
            cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }

    /// <inheritdoc />
    public async Task<FargoResponse> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/articles/{articleGuid}", cancellationToken);

        return FargoResponseMapper.Map(httpResponse);
    }
}
