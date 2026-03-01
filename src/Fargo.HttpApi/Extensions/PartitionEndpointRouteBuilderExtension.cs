using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    public static class PartitionEndpointRouteBuilderExtension
    {
        public static void MapFargoPartition(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/partition")
                .RequireAuthorization();

            group.MapGet("/{partitionGuid}", GetSinglePartition);
            group.MapGet("/", GetManyPartitions);
            group.MapPost("/", CreatePartition);
            group.MapPatch("/{partitionGuid}", UpdatePartition);
            group.MapDelete("/{partitionGuid}", DeletePartition);
        }

        private static async Task<Results<Ok<PartitionReadModel>, NotFound>> GetSinglePartition(
            Guid partitionGuid,
            DateTime? temporalAsOf,
            IQueryHandler<PartitionSingleQuery, PartitionReadModel?> handler,
            CancellationToken cancellationToken)
        {
            var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IEnumerable<PartitionReadModel>>, NotFound, NoContent>> GetManyPartitions(
            DateTime? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<PartitionManyQuery, IEnumerable<PartitionReadModel>> handler,
            CancellationToken cancellationToken)
        {
            var query = new PartitionManyQuery(
                temporalAsOf,
                new(page ?? default, limit ?? default)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Ok<Guid>> CreatePartition(
            PartitionCreateCommand command,
            ICommandHandler<PartitionCreateCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            var response = await handler.Handle(command, cancellationToken);

            return TypedResults.Ok(response);
        }

        private static async Task<NoContent> UpdatePartition(
            Guid partitionGuid,
            PartitionUpdateModel model,
            ICommandHandler<PartitionUpdateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new PartitionUpdateCommand(partitionGuid, model);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }

        private static async Task<NoContent> DeletePartition(
            Guid partitionGuid,
            ICommandHandler<PartitionDeleteCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new PartitionDeleteCommand(partitionGuid);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}