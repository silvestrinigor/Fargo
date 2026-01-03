using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Queries;
using Fargo.HttpApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Extensions
{
    public static class EndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet("/articles/{articleGuid}", async (Guid articleGuid, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto> handler)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid)));

                builder.MapGet("/articles", async ([FromServices] IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>> handler)
                    => await handler.HandleAsync(new ArticleAllQuery()));

                builder.MapPost("/articles", async ([FromBody] ArticleCreateCommand command, [FromServices] ICommandHandlerAsync<ArticleCreateCommand, Guid> handler)
                    => await handler.HandleAsync(command));

                builder.MapDelete("/articles/{articleGuid}", async (Guid articleGuid, [FromServices] ICommandHandlerAsync<ArticleDeleteCommand> handler)
                    => await handler.HandleAsync(new ArticleDeleteCommand(articleGuid)));
            }

            public void MapFargoItem()
            {
                builder.MapGet("/items/{itemGuid}", async (Guid itemGuid, [FromServices] IQueryHandlerAsync<ItemSingleQuery, ItemDto> handler)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid)));

                builder.MapGet("/items", async ([FromServices] IQueryHandlerAsync<ItemAllQuery, IEnumerable<ItemDto>> handler)
                    => await handler.HandleAsync(new ItemAllQuery()));

                builder.MapPost("/items", async ([FromBody] ItemCreateCommand command, [FromServices] ICommandHandlerAsync<ItemCreateCommand, Guid> handler)
                    => await handler.HandleAsync(command));

                builder.MapDelete("/items/{itemGuid}", async (Guid itemGuid, [FromServices] ICommandHandlerAsync<ItemDeleteCommand> handler)
                    => await handler.HandleAsync(new ItemDeleteCommand(itemGuid)));
            }
        }
    }
}