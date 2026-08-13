using Fargo.Application.Articles;
using Fargo.Application.Common;
using Fargo.Grpc.V1;
using Grpc.Core;

namespace Fargo.Grpc.Services;

public sealed class ArticleService(
    IQueryHandler<ArticleByGuidQuery, ArticleDto?> byGuidQuery
) : V1.ArticleService.ArticleServiceBase
{
    public override async Task<Article> GetArticle(GetArticleRequest request, ServerCallContext context)
    {
        var query = new ArticleByGuidQuery(Guid.Parse(request.ArticleGuid));

        var articleDto = await byGuidQuery.HandleAsync(query, context.CancellationToken)

        ?? throw new RpcException(new Status(StatusCode.NotFound, "Article was not found"));

        return await Task.FromResult(new Article
        {
            Guid = articleDto.Guid.ToString()
        });
    }
}
