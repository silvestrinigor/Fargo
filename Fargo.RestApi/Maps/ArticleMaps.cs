using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Maps;

public static class ArticleMaps
{
    public static void MapFagoArticle(this IEndpointRouteBuilder webApplication)
    {
        webApplication.MapGet("/Articles/{guid}", async (Guid guid, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.GetArticleAsync(guid));

        webApplication.MapGet("/Articles/", async ([FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.GetArticlesAsync());

        webApplication.MapPost("/Articles/", async ([FromBody] ArticleDto articleCreateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.CreateArticleAsync(articleCreateDto));

        webApplication.MapPatch("/Articles/", async (Guid guid, [FromBody] ArticleDto articleUpdateDto, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.UpdateArticleAsync(articleUpdateDto));

        webApplication.MapDelete("/Articles/{guid}", async (Guid guid, [FromServices] IArticleApplicationService articleApplicationService)
            => await articleApplicationService.DeleteArticleAsync(guid));
    }
}