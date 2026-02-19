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
                builder.MapGet(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            DateTime? temporalAsOf,
                            IQueryHandler<PartitionSingleQuery, PartitionReadModel?> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                builder.MapGet(
                        "/partition",
                        async (
                            DateTime? temporalAsOf,
                            Page? page,
                            Limit? limit,
                            IQueryHandler<PartitionManyQuery, IEnumerable<PartitionReadModel>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new PartitionManyQuery(temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                builder.MapPost(
                        "/partition",
                        async (
                            PartitionCreateCommand command,
                            ICommandHandler<PartitionCreateCommand, Guid> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var response = await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok(response);
                        });

                builder.MapDelete(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            ICommandHandler<PartitionDeleteCommand> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new PartitionDeleteCommand(partitionGuid);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });

                builder.MapPatch(
                        "/partition/{partitionGuid}",
                        async (
                            Guid partitionGuid,
                            PartitionUpdateModel model,
                            ICommandHandler<PartitionUpdateCommand> handler,
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