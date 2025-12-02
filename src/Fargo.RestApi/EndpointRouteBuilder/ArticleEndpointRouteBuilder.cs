using Fargo.Application.Interfaces.Services;
using Fargo.Application.Solicitations.Commands.ArticleCommands;
using Fargo.Application.Solicitations.Queries.ArticleQueries;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ArticleEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet("/articles/{article}", async (Guid article, [FromServices] IArticleService service)
                    => await service.GetArticleAsync(new ArticleQuery(article)));

                builder.MapPost("/articles", async ([FromBody] ArticleCreateCommand command, [FromServices] IArticleService service)
                    => await service.CreateArticleAsync(command));

                builder.MapDelete("/articles/{article}", async (Guid article, [FromServices] IArticleService service)
                    => await service.DeleteArticleAsync(new ArticleDeleteCommand(article)));
            }
        }
    }
}