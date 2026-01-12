using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.HttpApi.Commom;

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
                        DateTime? atDateTime,
                        IQueryHandlerAsync<PartitionSingleQuery, PartitionReadModel?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(new PartitionSingleQuery(partitionGuid, atDateTime), cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "/partition",
                    async (
                        DateTime? atDateTime,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new PartitionManyQuery(atDateTime, new Pagination(page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "/partition",
                    async (
                        PartitionCreateCommand command,
                        ICommandHandlerAsync<PartitionCreateCommand, Guid> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapPatch(
                    "/partition/{partitionGuid}",
                    async (
                        Guid partitionGuid,
                        PartitionUpdateModel model,
                        ICommandHandlerAsync<PartitionUpdateCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new PartitionUpdateCommand(partitionGuid, model), cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}