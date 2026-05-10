using Google.Protobuf.WellKnownTypes;
using Contracts = Fargo.GrpcContracts;

namespace Fargo.GrpcClient;

public sealed class FargoArticleGrpcClient(
    Contracts.ArticlesGrpc.ArticlesGrpcClient client,
    FargoGrpcCallExecutor calls)
{
    public Task<Contracts.ArticleInfo> GetArticleAsync(
        Contracts.GetArticleRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetArticleAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.ArticleInfo> GetArticleByBarcodeAsync(
        Contracts.GetArticleByBarcodeRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetArticleByBarcodeAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.ArticleList> GetArticlesAsync(
        Contracts.GetManyRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.GetArticlesAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Contracts.GuidResult> CreateArticleAsync(
        Contracts.ArticleCreateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.CreateArticleAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> UpdateArticleAsync(
        Contracts.ArticleUpdateRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.UpdateArticleAsync(request, headers, deadline, ct),
            cancellationToken);

    public Task<Empty> DeleteArticleAsync(
        Contracts.GuidRequest request,
        CancellationToken cancellationToken = default)
        => calls.UnaryAsync(
            (headers, deadline, ct) => client.DeleteArticleAsync(request, headers, deadline, ct),
            cancellationToken);
}
