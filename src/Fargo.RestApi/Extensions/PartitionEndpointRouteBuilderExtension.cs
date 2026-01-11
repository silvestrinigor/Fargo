using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.Domain.ValueObjects;
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
                    async (Guid partitionGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<PartitionSingleQuery, PartitionDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionSingleQuery(partitionGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/partition",
                    async ([FromQuery] DateTime? atDateTime, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionManyQuery(atDateTime, new Pagination(page, limit)), cancellationToken));

                builder.MapPost(
                    "/partition",
                    async ([FromBody] PartitionCreateCommand command, [FromServices] ICommandHandlerAsync<PartitionCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));
            }
        }
    }
}
