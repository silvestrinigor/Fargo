using Fargo.Application.Common.Paging;
using Fargo.Application.DependencyInjection;
using Fargo.Application.Commands.Articles;
using Fargo.Application.Dtos.Articles;
using Fargo.Application.Queries.Articles;
using Microsoft.Extensions.DependencyInjection;

namespace Fargo.Application.DependencyInjection;

public static class ArticleServiceCollectionExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddFargoArticleApplication() => services
        .AddFargoCommandHandler<ArticleCreateCommand, Guid, ArticleCreateCommandHandler>()
        .AddFargoCommandHandler<ArticleUpdateCommand, ArticleUpdateCommandHandler>()
        .AddFargoCommandHandler<ArticleSetPartitionsCommand, ArticleSetPartitionsCommandHandler>()
        .AddFargoCommandHandler<ArticleDeleteCommand, ArticleDeleteCommandHandler>()
        .AddFargoQueryHandler<ArticleByGuidQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticleByBarcodeQuery, ArticleDto?, ArticleQueryHandler>()
        .AddFargoQueryHandler<ArticlesQuery, PagedResult<ArticleDto>, ArticlesQueryHandler>()
        .AddFargoQueryHandler<ArticleInventoryByGuidQuery, ArticleInventoryDto?, ArticleInventoryQueryHandler>();
    }
}
