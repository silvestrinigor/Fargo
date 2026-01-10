using Fargo.Application.Dtos;
using Fargo.Application.Dtos.ArticleDtos;
using Fargo.Application.Dtos.ItemDtos;
using Fargo.Application.Dtos.PartitionDtos;
using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.ArticleCommands;
using Fargo.Application.Requests.Commands.ItemCommands;
using Fargo.Application.Requests.Commands.PartitionCommands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries.ArticleQueries;
using Fargo.Application.Requests.Queries.ItemQueries;
using Fargo.Application.Requests.Queries.PartitionQueries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.HttpApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Extensions
{
    public static class EndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoArticle()
            {
                builder.MapGet(
                    "/articles/{articleGuid}",
                    async (Guid articleGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<ArticleSingleQuery, ArticleDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleSingleQuery(articleGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/articles",
                    async ([FromQuery] DateTime? atDateTime, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<ArticleAllQuery, IEnumerable<ArticleDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleAllQuery(atDateTime, new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/articles",
                    async ([FromBody] ArticleCreateCommand command, [FromServices] ICommandHandlerAsync<ArticleCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapPatch(
                    "/articles/{articleGuid}",
                    async (Guid articleGuid, [FromBody] ArticleUpdateDto dto, [FromServices] ICommandHandlerAsync<ArticleUpdateCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleUpdateCommand(articleGuid, dto), cancellationToken));

                builder.MapDelete(
                    "/articles/{articleGuid}",
                    async (Guid articleGuid, [FromServices] ICommandHandlerAsync<ArticleDeleteCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ArticleDeleteCommand(articleGuid), cancellationToken));
            }

            public void MapFargoItem()
            {
                builder.MapGet(
                    "/items/{itemGuid}",
                    async (Guid itemGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<ItemSingleQuery, ItemDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemSingleQuery(itemGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/items",
                    async ([FromQuery] Guid? parentItemGuid, [FromQuery] Guid? articleGuid, [FromQuery] DateTime? atDateTime, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<ItemManyQuery, IEnumerable<ItemDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemManyQuery(parentItemGuid, articleGuid, atDateTime, new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/items",
                    async ([FromBody] ItemCreateCommand command, [FromServices] ICommandHandlerAsync<ItemCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapPatch(
                    "/items/{itemGuid}",
                    async (Guid itemGuid, [FromBody] ItemUpdateDto dto, [FromServices] ICommandHandlerAsync<ItemUpdateCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemUpdateCommand(itemGuid, dto), cancellationToken));

                builder.MapDelete(
                    "/items/{itemGuid}",
                    async (Guid itemGuid, [FromServices] ICommandHandlerAsync<ItemDeleteCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new ItemDeleteCommand(itemGuid), cancellationToken));
            }

            public void MapFargoUser()
            {
                builder.MapGet(
                    "/users/{userGuid}",
                    async (Guid userGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<UserSingleQuery, UserDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserSingleQuery(userGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/users",
                    async ([FromQuery] int? page, [FromQuery] DateTime? atDateTime, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<UserAllQuery, IEnumerable<UserDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserAllQuery(atDateTime, new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/users",
                    async ([FromBody] UserCreateCommand command, [FromServices] ICommandHandlerAsync<UserCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapDelete(
                    "/users/{userGuid}",
                    async (Guid userGuid, [FromServices] ICommandHandlerAsync<UserDeleteCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserDeleteCommand(userGuid), cancellationToken));

                builder.MapGet(
                    "/users/{userGuid}/permissions",
                    async (Guid userGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<UserPermissionDto>?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionAllQuery(userGuid, atDateTime), cancellationToken));

                builder.MapPatch(
                    "/users/{userGuid}/permissions",
                    async (Guid userGuid, [FromBody] UserPermissionDto permission, [FromServices] ICommandHandlerAsync<UserPermissionUpdateCommand> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionUpdateCommand(userGuid, permission), cancellationToken));
            }

            public void MapFargoPartition()
            {
                builder.MapGet(
                    "/partition/{partitionGuid}",
                    async (Guid partitionGuid, [FromQuery] DateTime? atDateTime, [FromServices] IQueryHandlerAsync<PartitionSingleQuery, PartitionDto?> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionSingleQuery(partitionGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/partition",
                    async ([FromQuery] DateTime? atDateTime, [FromQuery] int? page, [FromQuery] int? limit, [FromServices] IQueryHandlerAsync<PartitionManyQuery, IEnumerable<PartitionDto>> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(new PartitionManyQuery(atDateTime, new PaginationDto(page, limit)), cancellationToken));

                builder.MapPost(
                    "/partition",
                    async ([FromBody] PartitionCreateCommand command, [FromServices] ICommandHandlerAsync<PartitionCreateCommand, Guid> handler, CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));
            }
        }
    }
}