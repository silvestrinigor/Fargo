using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.HttpApi.Helpers;

namespace Fargo.HttpApi.Extensions
{
    public static class ItemEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoItem()
            {
                builder.MapGet(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        DateTime? temporalAsOf,
                        IQueryHandlerAsync<ItemSingleQuery, ItemReadModel?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ItemSingleQuery(itemGuid, temporalAsOf);

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "/items",
                    async (
                        Guid? parentItemGuid,
                        Guid? articleGuid,
                        DateTime? temporalAsOf,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ItemManyQuery(parentItemGuid, articleGuid, temporalAsOf, new (page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "/items",
                    async (
                        ItemCreateCommand command,
                        ICommandHandlerAsync<ItemCreateCommand, Guid> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapPatch(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ItemUpdateModel model,
                        ICommandHandlerAsync<ItemUpdateCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ItemUpdateCommand(itemGuid, model);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapDelete(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ICommandHandlerAsync<ItemDeleteCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ItemDeleteCommand(itemGuid);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}