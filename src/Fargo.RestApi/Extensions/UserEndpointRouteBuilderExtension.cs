using Fargo.Application.Dtos.UserDtos;
using Fargo.Application.Mediators;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Domain.ValueObjects;

namespace Fargo.HttpApi.Extensions
{
    public static class UserEndpointRouteBuilderExtension
    {
        extension(IEndpointRouteBuilder builder)
        {
            public void MapFargoUser()
            {
                builder.MapGet(
                    "/users/{userGuid}",
                    async (
                        Guid userGuid,
                        DateTime? atDateTime,
                        IQueryHandlerAsync<UserSingleQuery, UserDto?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserSingleQuery(userGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/users",
                    async (
                        DateTime? atDateTime,
                        int? page,
                        int? limit,
                        IQueryHandlerAsync<UserAllQuery, IEnumerable<UserDto>> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserAllQuery(atDateTime, new Pagination(page, limit)), cancellationToken));

                builder.MapPost(
                    "/users",
                    async (
                        UserCreateCommand command,
                        ICommandHandlerAsync<UserCreateCommand, Guid> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(command, cancellationToken));

                builder.MapDelete(
                    "/users/{userGuid}",
                    async (
                        Guid userGuid,
                        ICommandHandlerAsync<UserDeleteCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserDeleteCommand(userGuid), cancellationToken));

                builder.MapGet(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        DateTime? atDateTime,
                        IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<UserPermissionDto>?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionAllQuery(userGuid, atDateTime), cancellationToken));

                builder.MapPatch(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        UserPermissionDto permission,
                        ICommandHandlerAsync<UserPermissionUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionUpdateCommand(userGuid, permission), cancellationToken));
            }
        }
    }
}