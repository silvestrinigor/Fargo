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
                /// <summary>
                /// Retrieves a single item by its GUID.
                /// </summary>
                /// <param name="itemGuid">The unique identifier of the item.</param>
                /// <param name="temporalAsOf">Optional date and time to specify when the item's state should be retrieved.</param>
                /// <param name="handler">The query handler for retrieving a single item.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>A task that returns the item read model if found, or null if not found.</returns>
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

                /// <summary>
                /// Retrieves a list of items based on optional filters and pagination.
                /// </summary>
                /// <param name="parentItemGuid">Optional GUID to filter items by their parent.</param>
                /// <param name="articleGuid">Optional GUID to filter items by article.</param>
                /// <param name="temporalAsOf">Optional date and time to specify when the item's state should be retrieved.</param>
                /// <param name="page">Page number for pagination. Defaults to 1.</param>
                /// <param name="limit">Number of items per page. Defaults to 10.</param>
                /// <param name="handler">The query handler for retrieving multiple items.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>A task that returns a list of item read models.</returns>
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
                        var query = new ItemManyQuery(
                                parentItemGuid,
                                articleGuid,
                                temporalAsOf,
                                new(page ?? default, limit ?? default)
                                );

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                /// <summary>
                /// Creates a new item.
                /// </summary>
                /// <param name="command">The command containing the data for the new item.</param>
                /// <param name="handler">The command handler for creating an item.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>A task that returns the GUID of the created item.</returns>
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

                /// <summary>
                /// Updates an existing item.
                /// </summary>
                /// <param name="itemGuid">The unique identifier of the item to update.</param>
                /// <param name="model">The model containing the updated data for the item.</param>
                /// <param name="handler">The command handler for updating an item.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>A task that completes when the update is successful.</returns>
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

                /// <summary>
                /// Deletes an existing item.
                /// </summary>
                /// <param name="itemGuid">The unique identifier of the item to delete.</param>
                /// <param name="handler">The command handler for deleting an item.</param>
                /// <param name="cancellationToken">Token to cancel the operation.</param>
                /// <returns>A task that completes when the deletion is successful.</returns>
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