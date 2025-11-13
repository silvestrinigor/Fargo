using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class PartitionEndpointRouteBuilder
    {
        public static void MapFargoPartition(this IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapGet("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionAsync(partition));

            endpointRouteBuilder.MapGet("/partitions", async ([FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionAsync());

            endpointRouteBuilder.MapGet("/partitions/guids", async ([FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionGuidsAsync());

            endpointRouteBuilder.MapPost("/partitions", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.CreatePartitoinAsync(articleCreateDto));

            endpointRouteBuilder.MapPatch("/partitions/{partition}", async (Guid partition, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.UpdatePartitionAsync(partition, containerUpdateDto));

            endpointRouteBuilder.MapDelete("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.DeletePartitionAsync(partition));

            endpointRouteBuilder.MapGet("/partitions/{partition}/entities", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionEntitiesAsync(partition));

            endpointRouteBuilder.MapPut("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.InsertEntityIntoPartitionAsync(partition, entity));

            endpointRouteBuilder.MapDelete("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.RemoveEntityFromPartitionAsync(partition, entity));
        }
    }
}