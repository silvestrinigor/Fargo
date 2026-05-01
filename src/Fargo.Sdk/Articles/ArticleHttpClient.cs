using Fargo.Sdk.Contracts.Articles;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Http;
using Fargo.Sdk.Partitions;

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
    public async Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoHttpClient.BuildQuery(("temporalAsOf", temporalAsOf?.ToString("O")));
        var httpResponse = await httpClient.GetAsync<ArticleDto>($"/articles/{articleGuid}{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ArticleResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ArticleResult>(httpResponse.Data!.ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
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

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ArticleDto>>($"/articles{query}", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>((httpResponse.Data ?? []).ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<ArticleCreateRequest, Guid>(
            "/articles",
            ContractMappings.ToArticleCreateRequest(name, description, firstPartition, metrics, shelfLife),
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
        string? name = null,
        string? description = null,
        ArticleMetrics? metrics = null,
        TimeSpan? shelfLife = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync<ArticleUpdateRequest>(
            $"/articles/{articleGuid}",
            ContractMappings.ToArticleUpdateRequest(name, description, metrics, shelfLife),
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

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> AddPartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostJsonAsync(
            $"/articles/{articleGuid}/partitions/{partitionGuid}",
            new { },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> RemovePartitionAsync(
        Guid articleGuid,
        Guid partitionGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync(
            $"/articles/{articleGuid}/partitions/{partitionGuid}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<IReadOnlyCollection<PartitionResult>>> GetPartitionsAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<PartitionDto>>(
            $"/articles/{articleGuid}/partitions",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<PartitionResult>>((httpResponse.Data ?? []).ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UploadImageAsync(
        Guid articleGuid,
        Stream stream,
        string contentType,
        string fileName = "image",
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutMultipartAsync(
            $"/articles/{articleGuid}/image",
            stream,
            contentType,
            fileName,
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> DeleteImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync($"/articles/{articleGuid}/image", cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    /// <inheritdoc />
    public Task<(Stream Stream, string ContentType)?> GetImageAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
        => httpClient.GetStreamAsync($"/articles/{articleGuid}/image", cancellationToken);

    /// <inheritdoc />
    public async Task<FargoSdkResponse<ArticleBarcodes>> GetBarcodesAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.GetAsync<ArticleBarcodesDto>(
            $"/articles/{articleGuid}/barcodes",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ArticleBarcodes>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ArticleBarcodes>(httpResponse.Data.ToSdk());
    }

    /// <inheritdoc />
    public async Task<FargoSdkResponse<EmptyResult>> UpdateBarcodesAsync(
        Guid articleGuid,
        ArticleBarcodes barcodes,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PutJsonAsync(
            $"/articles/{articleGuid}/barcodes",
            barcodes.ToContract(),
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem) => FargoSdkProblemMapper.Map(problem);
}
