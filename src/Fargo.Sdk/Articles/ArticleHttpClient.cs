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
            return new FargoSdkResponse<ArticleInfo>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ArticleInfo>(httpResponse.Data!);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        Guid? partitionGuid = null,
        string? search = null,
        bool? noPartition = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()),
            ("partitionGuid", partitionGuid?.ToString()),
            ("search", search),
            ("noPartition", noPartition?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ArticleInfo>>($"/articles{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ArticleInfo>>(httpResponse.Data ?? []);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ArticleCreateRequest, Guid>(
            "/articles",
            ContractMappings.ToArticleCreateRequest(name, description, partitions, barcodes, metrics, shelfLife, isActive),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string name,
        string? description = null,
        IReadOnlyCollection<Guid>? partitions = null,
        ArticleBarcodes? barcodes = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        bool isActive = true,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync<ArticleUpdateRequest>(
            $"/articles/{articleGuid}",
            ContractMappings.ToArticleUpdateRequest(name, description, partitions, barcodes, metrics, shelfLife, isActive),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
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
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
