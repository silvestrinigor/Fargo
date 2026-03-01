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
    public static class ItemEndpointRouteBuilderExtension
    {
        public static void MapFargoItem(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/items")
                .RequireAuthorization();

            group.MapGet("/{itemGuid}", GetSingleItem);
            group.MapGet("/", GetManyItems);
            group.MapPost("/", CreateItem);
            group.MapPatch("/{itemGuid}", UpdateItem);
            group.MapDelete("/{itemGuid}", DeleteItem);
        }

        private static async Task<Results<Ok<ItemReadModel>, NotFound>> GetSingleItem(
            Guid itemGuid,
            DateTime? temporalAsOf,
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
            DateTime? temporalAsOf,
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