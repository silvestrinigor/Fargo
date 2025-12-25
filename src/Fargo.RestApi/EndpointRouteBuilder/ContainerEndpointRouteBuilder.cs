using Fargo.Application.Services;
using Fargo.Application.Solicitations.Commands.ContainerCommands;
using Fargo.Application.Solicitations.Queries.ContainerQueries;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.EndpointRouteBuilder
{
    public static class ContainerEndpointRouteBuilder
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoContainer()
            {
                builder.MapGet("/containers/{container}", async (Guid container, [FromServices] IContainerService service)
                    => await service.GetContainerAsync(new ContainerQuery(container)));

                builder.MapPost("/containers", async ([FromBody] ContainerCreateCommand command, [FromServices] IContainerService service)
                    => await service.CreateContainerAsync(command));

                builder.MapPost("/containers/{container}/childs/{entity}", async (Guid container, Guid entity, [FromServices] IContainerService service)
                    => await service.AddEntityInContainerAsync(new ContainerItemAddCommand(entity, container)));

                builder.MapGet("/containers/{container}/childs/guids", async (Guid container, [FromServices] IContainerService service)
                    => await service.GetChildEntitiesGuids(new ContainerChildEntitiesGuidQuery(container)));

                builder.MapDelete("/containers/{container}", async (Guid container, [FromServices] IContainerService service)
                    => await service.DeleteContainerAsync(new ContainerDeleteCommand(container)));
            }
        }
    }
}