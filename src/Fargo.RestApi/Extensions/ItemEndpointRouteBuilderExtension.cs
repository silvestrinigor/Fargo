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
                        IQueryHandler<ItemSingleQuery, Task<ItemReadModel?>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ItemSingleQuery(itemGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

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
                        IQueryHandler<ItemManyQuery, Task<IEnumerable<ItemReadModel>>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new ItemManyQuery(parentItemGuid, articleGuid, temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "/items",
                    async (
                        ItemCreateCommand command,
                        ICommandHandler<ItemCreateCommand, Task<Guid>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapPatch(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ItemUpdateModel model,
                        ICommandHandler<ItemUpdateCommand, Task> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ItemUpdateCommand(itemGuid, model);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapDelete(
                    "/items/{itemGuid}",
                    async (
                        Guid itemGuid,
                        ICommandHandler<ItemDeleteCommand, Task> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var command = new ItemDeleteCommand(itemGuid);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}