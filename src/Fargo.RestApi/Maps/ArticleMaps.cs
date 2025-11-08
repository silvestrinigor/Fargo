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

        webApplication.MapPost("/articles/", async ([FromBody] EntityDto articleCreateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.CreateArticleAsync(articleCreateDto));

        webApplication.MapPatch("/articles/", async ([FromBody] EntityDto articleUpdateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.UpdateArticleAsync(articleUpdateDto));

        webApplication.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.DeleteArticleAsync(article));
    }
}