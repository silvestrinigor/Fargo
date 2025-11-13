using Fargo.Application.Contracts;
using Fargo.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class PartitionEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoPartition()
            {
                builder.MapGet("/partitions", async ([FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.GetPartitionAsync());

                builder.MapGet("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.GetPartitionAsync(partition));

                builder.MapGet("/partitions/guids", async ([FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.GetPartitionGuidsAsync());

                builder.MapGet("/partitions/{partition}/entities", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.GetPartitionEntitiesAsync(partition));

                builder.MapPost("/partitions", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.CreatePartitoinAsync(articleCreateDto));

                builder.MapPatch("/partitions/{partition}", async (Guid partition, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.UpdatePartitionAsync(partition, containerUpdateDto));

                builder.MapPut("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.InsertEntityIntoPartitionAsync(partition, entity));

                builder.MapDelete("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.DeletePartitionAsync(partition));

                builder.MapDelete("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService containerApplicationService)
                    => await containerApplicationService.RemoveEntityFromPartitionAsync(partition, entity));
            }
        }
    }
}