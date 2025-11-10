using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Maps;

public static class ArticleMaps
{
    public static void MapFargoArticle(this IEndpointRouteBuilder webApplication)
    {
        webApplication.MapGet("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.GetArticleAsync(article));

        webApplication.MapGet("/articles/", async ([FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.GetArticlesAsync());

        webApplication.MapGet("/articles/guids/", async ([FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.GetArticlesGuidsAsync());

        webApplication.MapPost("/articles/", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.CreateArticleAsync(articleCreateDto));

        webApplication.MapPatch("/articles/{article}", async (Guid article, [FromBody] EntityUpdateDto articleUpdateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.UpdateArticleAsync(article, articleUpdateDto));

        webApplication.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.DeleteArticleAsync(article));
    }
}