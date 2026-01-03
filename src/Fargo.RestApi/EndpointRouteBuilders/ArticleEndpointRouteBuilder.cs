using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Queries;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilders
{
    public static class ArticleEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet("/articles/{articleGuid}", async (Guid articleGuid, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto> handler)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid)));

                builder.MapPost("/articles", async ([FromBody] ArticleCreateCommand command, [FromServices] ICommandHandlerAsync<ArticleCreateCommand, Guid> handler)
                    => await handler.HandleAsync(command));

                builder.MapDelete("/articles/{articleGuid}", async (Guid articleGuid, [FromServices] ICommandHandlerAsync<ArticleDeleteCommand> handler)
                    => await handler.HandleAsync(new ArticleDeleteCommand(articleGuid)));
            }
        }
    }
}