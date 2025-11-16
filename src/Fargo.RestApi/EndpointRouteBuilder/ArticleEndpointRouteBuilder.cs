using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ArticleEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet("/articles", async ([FromServices] IArticleApplicationService service)
                    => await service.GetArticlesAsync());

                builder.MapGet("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService service)
                    => await service.GetArticleAsync(article));

                builder.MapGet("/articles/guids", async ([FromServices] IArticleApplicationService articleApplicationService)
                    => await articleApplicationService.GetArticlesGuidsAsync());

                builder.MapPost("/articles", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IArticleApplicationService service)
                    => await service.CreateArticleAsync(articleCreateDto));

                builder.MapPatch("/articles/{article}", async (Guid article, [FromBody] EntityUpdateDto articleUpdateDto, [FromServices] IArticleApplicationService service)
                    => await service.UpdateArticleAsync(article, articleUpdateDto));

                builder.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleApplicationService service)
                    => await service.DeleteArticleAsync(article));
            }
        }
    }
}