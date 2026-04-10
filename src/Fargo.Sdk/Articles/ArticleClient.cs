using Fargo.Sdk.Http;

namespace Fargo.Sdk.Articles;

public sealed class ArticleClient : IArticleClient
{
    internal ArticleClient(IFargoSdkHttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    private readonly IFargoSdkHttpClient httpClient;

    public async Task<FargoSdkResponse<ArticleResult>> GetAsync(
        Guid articleGuid,
        DateTimeOffset? temporalAsOf = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")));

        var httpResponse = await httpClient.GetAsync<ArticleResult>(
            $"/articles/{articleGuid}{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<ArticleResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<ArticleResult>(httpResponse.Data!);
    }

    public async Task<FargoSdkResponse<IReadOnlyCollection<ArticleResult>>> GetManyAsync(
        DateTimeOffset? temporalAsOf = null,
        int? page = null,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var query = FargoSdkHttpClient.BuildQuery(
            ("temporalAsOf", temporalAsOf?.ToString("O")),
            ("page", page?.ToString()),
            ("limit", limit?.ToString()));

        var httpResponse = await httpClient.GetAsync<IReadOnlyCollection<ArticleResult>>(
            $"/articles{query}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<IReadOnlyCollection<ArticleResult>>(httpResponse.Data ?? []);
    }

    public async Task<FargoSdkResponse<Guid>> CreateAsync(
        string name,
        string? description = null,
        Guid? firstPartition = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PostFromJsonAsync<object, Guid>(
            "/articles",
            new
            {
                article = new
                {
                    name,
                    description,
                    firstPartition
                }
            },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<Guid>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<Guid>(httpResponse.Data);
    }

    public async Task<FargoSdkResponse<EmptyResult>> UpdateAsync(
        Guid articleGuid,
        string? name = null,
        string? description = null,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.PatchJsonAsync(
            $"/articles/{articleGuid}",
            new { name, description },
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    public async Task<FargoSdkResponse<EmptyResult>> DeleteAsync(
        Guid articleGuid,
        CancellationToken cancellationToken = default)
    {
        var httpResponse = await httpClient.DeleteAsync(
            $"/articles/{articleGuid}",
            cancellationToken);

        if (!httpResponse.IsSuccess)
        {
            return new FargoSdkResponse<EmptyResult>(MapError(httpResponse.Problem));
        }

        return new FargoSdkResponse<EmptyResult>();
    }

    private static FargoSdkError MapError(FargoProblemDetails? problem)
    {
        var type = problem?.Type switch
        {
            "article/not-found" => FargoSdkErrorType.NotFound,
            "auth/unauthorized" => FargoSdkErrorType.UnauthorizedAccess,
            "user/forbidden"
                or "partition/access-denied"
                or "entity/access-denied" => FargoSdkErrorType.Forbidden,
            "request/invalid"
                or "article/delete-with-items" => FargoSdkErrorType.InvalidInput,
            _ => FargoSdkErrorType.Undefined
        };

        var detail = problem?.Detail ?? "An unexpected error occurred.";

        return new FargoSdkError(type, detail);
    }
}
