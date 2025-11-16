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
                builder.MapGet("/partitions", async ([FromServices] IPartitionApplicationService service)
                    => await service.GetPartitionAsync());

                builder.MapGet("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService service)
                    => await service.GetPartitionAsync(partition));

                builder.MapGet("/partitions/guids", async ([FromServices] IPartitionApplicationService service)
                    => await service.GetPartitionGuidsAsync());

                builder.MapGet("/partitions/{partition}/entities", async (Guid partition, [FromServices] IPartitionApplicationService service)
                    => await service.GetPartitionEntitiesAsync(partition));

                builder.MapPost("/partitions", async ([FromBody] EntityCreateDto articleCreateDto, [FromServices] IPartitionApplicationService service)
                    => await service.CreatePartitoinAsync(articleCreateDto));

                builder.MapPatch("/partitions/{partition}", async (Guid partition, [FromBody] EntityUpdateDto containerUpdateDto, [FromServices] IPartitionApplicationService service)
                    => await service.UpdatePartitionAsync(partition, containerUpdateDto));

                builder.MapPut("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService service)
                    => await service.InsertEntityIntoPartitionAsync(partition, entity));

                builder.MapDelete("/partitions/{partition}", async (Guid partition, [FromServices] IPartitionApplicationService service)
                    => await service.DeletePartitionAsync(partition));

                builder.MapDelete("/partitions/{partition}/entities/{entity}", async (Guid partition, Guid entity, [FromServices] IPartitionApplicationService service)
                    => await service.RemoveEntityFromPartitionAsync(partition, entity));
            }
        }
    }
}