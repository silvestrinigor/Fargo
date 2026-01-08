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
                builder.MapGet(
                    "/articles/{articleGuid}", 
                    async (Guid articleGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/articles",
                    async ([FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleAllQuery(new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/articles", 
                    async ([FromBody] ArticleCreateCommand command, [FromServices] ICommandHandlerAsync<ArticleCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapDelete(
                    "/articles/{articleGuid}", 
                    async (Guid articleGuid, [FromServices] ICommandHandlerAsync<ArticleDeleteCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleDeleteCommand(articleGuid), cancellationToken));
            }

            public void MapFargoItem()
            {
                builder.MapGet(
                    "/items/{itemGuid}",
                    async (Guid itemGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<ItemSingleQuery, ItemDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/items", 
                    async ([FromQuery] Guid? articleGuid, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemManyQuery(articleGuid, new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/items", 
                    async ([FromBody] ItemCreateCommand command, [FromServices] ICommandHandlerAsync<ItemCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapDelete(
                    "/items/{itemGuid}", 
                    async (Guid itemGuid, [FromServices] ICommandHandlerAsync<ItemDeleteCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemDeleteCommand(itemGuid), cancellationToken));
            }

            public void MapFargoEvent()
            {
                builder.MapGet(
                    "/events/{eventGuid}", 
                    async (Guid eventGuid, [FromServices] IQueryHandlerAsync<EventSingleQuery, EventDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new EventSingleQuery(eventGuid), cancellationToken));

                builder.MapGet(
                    "/events",
                    async ([FromQuery] Guid? modelGuid, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<EventAllFromEntityQuery, IEnumerable<EventDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new EventAllFromEntityQuery(modelGuid, new PaginationDto(page,limit)), cancellationToken));
            }
        }
    }
}