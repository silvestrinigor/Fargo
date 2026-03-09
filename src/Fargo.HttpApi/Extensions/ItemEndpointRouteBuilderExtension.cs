using Fargo.Application.Commom;
using Fargo.Application.Models.ItemModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Extension responsible for mapping all Item endpoints.
    /// </summary>
    public static class ItemEndpointRouteBuilderExtension
    {
        /// <summary>
        /// Maps all routes related to items.
        /// </summary>
        /// <param name="builder">The endpoint route builder.</param>
        public static void MapFargoItem(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/items")
                .RequireAuthorization()
                .WithTags("Items");

            group.MapGet("/{itemGuid:guid}", GetSingleItem)
                .WithName("GetItem")
                .WithSummary("Gets a single item")
                .WithDescription("Retrieves a single item by its unique identifier. Supports querying historical data using temporal tables.")
                .Produces<ItemReadModel>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", GetManyItems)
                .WithName("GetItems")
                .WithSummary("Gets multiple items")
                .WithDescription("Retrieves a paginated list of items with optional filters such as parent item or article.")
                .Produces<IEnumerable<ItemReadModel>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapPost("/", CreateItem)
                .WithName("CreateItem")
                .WithSummary("Creates a new item")
                .WithDescription("Creates a new item and returns the generated identifier.")
                .Produces<Guid>(StatusCodes.Status200OK);

            group.MapPatch("/{itemGuid:guid}", UpdateItem)
                .WithName("UpdateItem")
                .WithSummary("Updates an existing item")
                .WithDescription("Updates an item using partial data.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            group.MapDelete("/{itemGuid:guid}", DeleteItem)
                .WithName("DeleteItem")
                .WithSummary("Deletes an item")
                .WithDescription("Deletes the specified item from the system.")
                .Produces(StatusCodes.Status204NoContent)
                .Produces(StatusCodes.Status404NotFound);

            ;
        }

        private static async Task<Results<Ok<ItemReadModel>, NotFound>> GetSingleItem(
            Guid itemGuid,
            DateTimeOffset? temporalAsOf,
            IQueryHandler<ItemSingleQuery, ItemReadModel?> handler,
            CancellationToken cancellationToken)
        {
            var query = new ItemSingleQuery(itemGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IEnumerable<ItemReadModel>>, NotFound, NoContent>> GetManyItems(
            Guid? parentItemGuid,
            Guid? articleGuid,
            DateTimeOffset? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<ItemManyQuery, IEnumerable<ItemReadModel>> handler,
            CancellationToken cancellationToken)
        {
            var query = new ItemManyQuery(
                parentItemGuid,
                articleGuid,
                temporalAsOf,
                new(page ?? default, limit ?? default)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Ok<Guid>> CreateItem(
            ItemCreateCommand command,
            ICommandHandler<ItemCreateCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            var response = await handler.Handle(command, cancellationToken);

            return TypedResults.Ok(response);
        }

        private static async Task<NoContent> UpdateItem(
            Guid itemGuid,
            ItemUpdateModel model,
            ICommandHandler<ItemUpdateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new ItemUpdateCommand(itemGuid, model);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }

        private static async Task<NoContent> DeleteItem(
            Guid itemGuid,
            ICommandHandler<ItemDeleteCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new ItemDeleteCommand(itemGuid);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}