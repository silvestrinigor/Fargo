using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Maps
{
    public static class PartitionMaps
    {
        public static void MapFargoPartition(this IEndpointRouteBuilder webApplication)
        {
            webApplication.MapGet("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionAsync(partition));

            webApplication.MapGet("/partitions/", async ([FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionAsync());

            webApplication.MapGet("/partitions/guids/", async ([FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionGuidsAsync());

            webApplication.MapPost("/partitions/", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.CreatePartitoinAsync(articleCreateDto));

            webApplication.MapPatch("/partitions/{partition}", async (Guid partition, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.UpdatePartitionAsync(partition, containerUpdateDto));

            webApplication.MapDelete("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.DeletePartitionAsync(partition));

            webApplication.MapGet("/partitions/{partition}/entities/", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.GetPartitionEntitiesAsync(partition));

            webApplication.MapPut("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.InsertEntityIntoPartitionAsync(partition, entity));

            webApplication.MapDelete("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                => await containerApplicationService.RemoveEntityFromPartitionAsync(partition, entity));
        }
    }
}

