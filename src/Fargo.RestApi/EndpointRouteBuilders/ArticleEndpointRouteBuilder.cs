using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Mediators;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilders
{
    public static class ArticleEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet("/articles/{article}", async (Guid article, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto> handler)
                    => await handler.HandleAsync(new ArticleSingleQuery(article)));

                builder.MapPost("/articles", async ([FromBody] ArticleCreateCommand command, [FromServices] ICommandHandlerAsync<ArticleCreateCommand, Guid> handler)
                    => await handler.HandleAsync(command));

                builder.MapDelete("/articles/{article}", async (Guid article, [FromServices] ICommandHandlerAsync<ArticleDeleteCommand> handler)
                    => await handler.HandleAsync(new ArticleDeleteCommand(article)));
            }
        }
    }
}