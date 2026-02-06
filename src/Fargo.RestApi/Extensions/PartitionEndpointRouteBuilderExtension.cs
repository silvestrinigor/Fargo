using Fargo.Application.Commom;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.HttpApi.Helpers;

namespace Fargo.HttpApi.Extensions
{
    public static class PartitionEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoPartition()
            {
                /// <summary>
                /// Gets a partition by its GUID.
                /// </summary>
                /// <param name="partitionGuid">The GUID of the partition to get.</param>
                /// <param name="temporalAsOf">Optional temporal timestamp for querying as of a specific time.</param>
                /// <param name="handler">Query handler for fetching the partition data.</param>
                /// <param name="cancellationToken">Token for cancellation of the request.</param>
                /// <returns>A task that represents the asynchronous operation. The result contains the partition read model or null if not found.</returns>
                builder.MapGet(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            DateTime? temporalAsOf,
                            IQueryHandler<PartitionSingleQuery, Task<PartitionReadModel?>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                /// <summary>
                /// Gets all partitions with optional filtering and pagination.
                /// </summary>
                /// <param name="temporalAsOf">Optional temporal timestamp for querying as of a specific time.</param>
                /// <param name="page">Optional page number for pagination.</param>
                /// <param name="limit">Optional limit on the number of results per page.</param>
                /// <param name="handler">Query handler for fetching the partition data.</param>
                /// <param name="cancellationToken">Token for cancellation of the request.</param>
                /// <returns>A task that represents the asynchronous operation. The result contains a collection of partition read models.</returns>
                builder.MapGet(
                        "/partition",
                        async (
                            DateTime? temporalAsOf,
                            Page? page,
                            Limit? limit,
                            IQueryHandler<PartitionManyQuery, Task<IEnumerable<PartitionReadModel>>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new PartitionManyQuery(temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                /// <summary>
                /// Creates a new partition.
                /// </summary>
                /// <param name="command">The command to create the partition.</param>
                /// <param name="handler">Command handler for creating the partition.</param>
                /// <param name="cancellationToken">Token for cancellation of the request.</param>
                /// <returns>A task that represents the asynchronous operation. The result contains the GUID of the created partition.</returns>
                builder.MapPost(
                        "/partition",
                        async (
                            PartitionCreateCommand command,
                            ICommandHandler<PartitionCreateCommand, Task<Guid>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var response = await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok(response);
                        });

                /// <summary>
                /// Deletes a partition by its GUID.
                /// </summary>
                /// <param name="partitionGuid">The GUID of the partition to delete.</param>
                /// <param name="handler">Command handler for deleting the partition.</param>
                /// <param name="cancellationToken">Token for cancellation of the request.</param>
                /// <returns>A task that represents the asynchronous operation. No content is returned on success.</returns>
                builder.MapDelete(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            ICommandHandler<PartitionDeleteCommand, Task> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new PartitionDeleteCommand(partitionGuid);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });

                /// <summary>
                /// Updates a partition by its GUID.
                /// </summary>
                /// <param name="partitionGuid">The GUID of the partition to update.</param>
                /// <param name="model">The model containing the updated partition data.</param>
                /// <param name="handler">Command handler for updating the partition.</param>
                /// <param name="cancellationToken">Token for cancellation of the request.</param>
                /// <returns>A task that represents the asynchronous operation. No content is returned on success.</returns>
                builder.MapPatch(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            PartitionUpdateModel model,
                            ICommandHandler<PartitionUpdateCommand, Task> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new PartitionUpdateCommand(partitionGuid, model);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });
            }
        }
    }
}