using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Operations.Articles.Dtos;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.Operations.Articles;

public static class ArticleServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoArticleApplication() => services
        .AddFargoCommandHandler<ArticleCreateCommand, Guid, ArticleCreateCommandHandler>()
        .AddFargoCommandHandler<ArticleUpdateCommand, ArticleUpdateCommandHandler>()
        .AddFargoCommandHandler<ArticleDeleteCommand, ArticleDeleteCommandHandler>()
        .AddFargoQueryHandler<ArticleByGuidQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticleByBarcodeQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticlesQuery, PagedResult<ArticleDto>, ArticlesQueryHandler>()
        .AddFargoQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?, ArticleInventoryQueryHandler>();
    }
}
