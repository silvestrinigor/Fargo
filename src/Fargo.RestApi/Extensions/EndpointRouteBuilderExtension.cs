using Fargo.Application.Dtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Queries;
using Fargo.Domain.Enums;
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
                    async (Guid articleGuid, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid), cancellationToken));

                builder.MapGet(
                    "/articles", 
                    async ([FromServices] IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleAllQuery(), cancellationToken));

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
                    async (Guid itemGuid, [FromServices] IQueryHandlerAsync<ItemSingleQuery, ItemDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid), cancellationToken));

                builder.MapGet(
                    "/items", 
                    async ([FromQuery] Guid? articleGuid, [FromServices] IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemManyQuery(articleGuid), cancellationToken));

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
                    async ([FromQuery] Guid? modelGuid, [FromServices] IQueryHandlerAsync<EventAllFromEntityQuery, IEnumerable<EventDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new EventAllFromEntityQuery(modelGuid), cancellationToken));
            }

            public void MapFargoModel()
            {
                builder.MapGet(
                    "/models/{modelGuid}", 
                    async (Guid modelGuid, [FromServices] IQueryHandlerAsync<ModelSingleQuery, ModelDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ModelSingleQuery(modelGuid), cancellationToken));

                builder.MapGet(
                    "/models", 
                    async ([FromQuery] ModelType? modelType, [FromServices] IQueryHandlerAsync<ModelManyQuery, IEnumerable<ModelDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ModelManyQuery(modelType), cancellationToken));
            }
        }
    }
}