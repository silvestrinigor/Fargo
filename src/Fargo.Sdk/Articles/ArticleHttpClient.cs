using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Http;

namespace Fargo.Sdk.Articles;

/// <summary>Default implementation of <see cref="IArticleHttpClient"/>.</summary>
public sealed class ArticleHttpClient : IArticleHttpClient
{
    /// <summary>Initializes a new instance with the given HTTP client.</summary>
    /// <param name="httpClient">The Fargo HTTP client used to make requests.</param>
    public ArticleHttpClient(IFargoHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoHttpClient httpClient;

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ArticleInfo>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ArticleInfo>($"/articles/{articleGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ArticleInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<ArticleInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ArticleInfo>> GetByBarcodeAsync(
        ArticleBarcode articleBarcode,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var pathBarcode = Uri.EscapeDataString(articleBarcode.Barcode);
        var httpResponse = await httpClient.GetAsync<ArticleInfo>(
            $"/articles/{pathBarcode}:{articleBarcode.Type}{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ArticleInfo>(MapError(httpResponse));
        }

        return new FargoSdkResponse<ArticleInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions = null,
        bool? notInsideAnyPartition = null,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<(string Key, string? Value)>
        {
            ("temporalAsOfDateTime", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("notInsideAnyPartition", notInsideAnyPartition?.ToString())
        };
        parameters.AddRange(insideAnyOfThisPartitions?.Select(partitionGuid =>
            ("insideAnyOfThisPartitions", (string?)partitionGuid.ToString())) ?? []);
        var query = FargoHttpClient.BuildQuery([.. parameters]);

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ArticleInfo>>($"/articles{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>(MapError(httpResponse));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        ArticleCreateRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ArticleCreateRequest, Guid>(
            "/articles",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        ArticleUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<ArticleUpdateRequest>(
            $"/articles/{articleGuid}",
            request,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/articles/{articleGuid}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError<T>(FargoSdkHttpResponse<T> response) => FargoSdkProblemMapper.Map(response.Problem, response.StatusCode);
}
