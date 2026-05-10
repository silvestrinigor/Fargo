using Fargo.Application;
using Fargo.Application.Articles;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Fargo.GrpcApi.Services;

[Authorize]
public sealed class ArticleGrpcService(
    IQueryHandler<ArticleSingleQuery, ArticleDto?> singleHandler,
    IQueryHandler<ArticleByBarcodeQuery, ArticleDto?> byBarcodeHandler,
    IQueryHandler<ArticlesQuery, IReadOnlyCollection<ArticleDto>> manyHandler,
    ICommandHandler<ArticleCreateCommand, Guid> createHandler,
    ICommandHandler<ArticleUpdateCommand> updateHandler,
    ICommandHandler<ArticleDeleteCommand> deleteHandler)
    : GrpcContracts.ArticlesGrpc.ArticlesGrpcBase
{
    public override async Task<GrpcContracts.ArticleInfo> GetArticle(
        GrpcContracts.GetArticleRequest request,
        ServerCallContext context)
    {
        var articleGuid = request.ArticleGuid.ToGuid(nameof(request.ArticleGuid));
        var result = await singleHandler.Handle(
            new ArticleSingleQuery(articleGuid, request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        return result?.ToInfo() ?? throw new ArticleNotFoundFargoApplicationException(articleGuid);
    }

    public override async Task<GrpcContracts.ArticleInfo> GetArticleByBarcode(
        GrpcContracts.GetArticleByBarcodeRequest request,
        ServerCallContext context)
    {
        var result = await byBarcodeHandler.Handle(
            new ArticleByBarcodeQuery(
                request.ArticleBarcode.ToApplicationDto(),
                request.TemporalAsOf.ToDateTimeOffset()),
            context.CancellationToken);

        if (result is null)
        {
            throw new RpcException(new Status(StatusCode.NotFound, "Article was not found."));
        }

        return result.ToInfo();
    }

    public override async Task<GrpcContracts.ArticleList> GetArticles(
        GrpcContracts.GetManyRequest request,
        ServerCallContext context)
    {
        var result = await manyHandler.Handle(
            new ArticlesQuery(
                request.ToPagination(),
                request.TemporalAsOf.ToDateTimeOffset(),
                request.InsideAnyOfThisPartitions.ToGuidCollectionOrNull(),
                request.HasNotInsideAnyPartition ? request.NotInsideAnyPartition : null),
            context.CancellationToken);

        var response = new GrpcContracts.ArticleList();
        response.Articles.AddRange(result.Select(static article => article.ToInfo()));
        return response;
    }

    public override async Task<GrpcContracts.GuidResult> CreateArticle(
        GrpcContracts.ArticleCreateRequest request,
        ServerCallContext context)
    {
        var guid = await createHandler.Handle(
            new ArticleCreateCommand(request.ToApplicationDto()),
            context.CancellationToken);

        return guid.ToGuidResult();
    }

    public override async Task<Empty> UpdateArticle(
        GrpcContracts.ArticleUpdateRequest request,
        ServerCallContext context)
    {
        await updateHandler.Handle(
            new ArticleUpdateCommand(
                request.ArticleGuid.ToGuid(nameof(request.ArticleGuid)),
                request.ToApplicationDto()),
            context.CancellationToken);

        return new Empty();
    }

    public override async Task<Empty> DeleteArticle(
        GrpcContracts.GuidRequest request,
        ServerCallContext context)
    {
        await deleteHandler.Handle(
            new ArticleDeleteCommand(request.Guid.ToGuid(nameof(request.Guid))),
            context.CancellationToken);

        return new Empty();
    }
}
