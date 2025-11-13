using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ArticleEndpointRouteBuilder
    {
        public static void MapFargoArticle(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticleAsync(article));

            endpointRouteBuilder.MapGet("/articles", async ([FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticlesAsync());

            endpointRouteBuilder.MapGet("/articles/guids", async ([FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.GetArticlesGuidsAsync());

            endpointRouteBuilder.MapPost("/articles", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.CreateArticleAsync(articleCreateDto));

            endpointRouteBuilder.MapPatch("/articles/{article}", async (Guid article, [FromBody] EntityUpdateDto articleUpdateDto, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.UpdateArticleAsync(article, articleUpdateDto));

            endpointRouteBuilder.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
                => await articleApplicationService.DeleteArticleAsync(article));
        }
    }
}