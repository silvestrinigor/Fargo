using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ArticleEndpointRouteBuilder
    {
        public static void MapFargoArticle(this IEndpointRouteBuilder builder)
        {
            builder.MapGet("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticleAsync(article));

            builder.MapGet("/articles", async ([FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticlesAsync());

            builder.MapGet("/articles/guids", async ([FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticlesGuidsAsync());

            builder.MapPost("/articles", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.CreateArticleAsync(articleCreateDto));

            builder.MapPatch("/articles/{article}", async (Guid article, [FromBody] EntityUpdateDto articleUpdateDto, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.UpdateArticleAsync(article, articleUpdateDto));

            builder.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.DeleteArticleAsync(article));
        }
    }
}