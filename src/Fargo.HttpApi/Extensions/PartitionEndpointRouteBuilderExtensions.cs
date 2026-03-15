using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    /// <summary>
    /// Extension responsible for mapping all Partition endpoints.
    /// </summary>
    public static class PartitionEndpointRouteBuilderExtension
    {
        /// <summary>
        /// Maps all routes related to partitions.
        /// </summary>
        /// <param name="builder">The endpoint route builder.</param>
        public static void MapFargoPartition(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/partitions")
                .RequireAuthorization()
                .WithTags("Partitions");

            group.MapGet("/{partitionGuid:guid}", GetSinglePartition)
                .WithName("GetPartition")
                .WithSummary("Gets a single partition")
                .WithDescription("Retrieves a single partition by its unique identifier. Optionally allows querying historical data using temporal tables.")
                .Produces<PartitionInformation>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status404NotFound);

            group.MapGet("/", GetManyPartition)
                .WithName("GetPartitions")
                .WithSummary("Gets multiple partitions")
                .WithDescription("Retrieves a paginated list of partitions. Supports optional temporal queries.")
                .Produces<IReadOnlyCollection<PartitionInformation>>(StatusCodes.Status200OK)
                .Produces(StatusCodes.Status204NoContent);

            group.MapPost("/", CreatePartition)
                .WithName("CreatePartition")
                .WithSummary("Creates a new partition")
                .WithDescription("Creates a new partition and returns the generated identifier.")
                .Produces<Guid>(StatusCodes.Status200OK);

            group.MapPatch("/{partitionGuid:guid}", UpdatePartition)
                .WithName("UpdatePartition")
                .WithSummary("Updates an existing partition")
                .WithDescription("Updates a partition using partial data.")
                .Produces(StatusCodes.Status204NoContent);

            group.MapDelete("/{partitionGuid:guid}", DeletePartition)
                .WithName("DeletePartition")
                .WithSummary("Deletes a partition")
                .WithDescription("Deletes the specified partition from the system.")
                .Produces(StatusCodes.Status204NoContent);
        }

        private static async Task<Results<Ok<PartitionInformation>, NotFound>> GetSinglePartition(
            Guid partitionGuid,
            DateTimeOffset? temporalAsOf,
            IQueryHandler<PartitionSingleQuery, PartitionInformation?> handler,
            CancellationToken cancellationToken)
        {
            var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NoContent>> GetManyPartition(
            Guid? parentPartitionGuid,
            DateTimeOffset? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>> handler,
            CancellationToken cancellationToken)
        {
            var query = new PartitionManyQuery(
                parentPartitionGuid,
                temporalAsOf,
                PaginationHelpers.CreatePagination(page, limit)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleCollectionQueryResult(response);
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