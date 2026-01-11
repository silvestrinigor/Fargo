using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.PartitionModels;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Microsoft.AspNetCore.Mvc;

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
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionSingleQuery(partitionGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/partition",
                    async (
                        DateTime? atDateTime, 
                        int? page, 
                        int? limit, 
                        IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionReadModel>> handler, 
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionManyQuery(atDateTime, new Pagination(page, limit)), cancellationToken));

                builder.MapPost(
                    "/partition",
                    async (
                        PartitionCreateCommand command, 
                        ICommandHandlerAsync<PartitionCreateCommand, Guid> handler, 
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapPatch(
                    "/partition/{partitionGuid}",
                    async (
                        Guid partitionGuid,
                        PartitionUpdateModel model,
                        ICommandHandlerAsync<PartitionUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionUpdateCommand(partitionGuid, model), cancellationToken));
            }
        }
    }
}