using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries.UserQueries;

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
                        IQueryHandlerAsync<UserSingleQuery, UserReadModel?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserSingleQuery(userGuid, atDateTime), cancellationToken));

                builder.MapGet(
                    "/users",
                    async (
                        DateTime? atDateTime,
                        int? page,
                        int? limit,
                        IQueryHandlerAsync<UserManyQuery, IEnumerable<UserReadModel>> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserManyQuery(atDateTime, new Pagination(page, limit)), cancellationToken));

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

                builder.MapPatch(
                    "/users/{userGuid}",
                    async (
                        Guid userGuid,
                        UserUpdateModel model,
                        ICommandHandlerAsync<UserUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserUpdateCommand(userGuid, model), cancellationToken));

                builder.MapGet(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        DateTime? atDateTime,
                        IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<PermissionReadModel>?> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionAllQuery(userGuid, atDateTime), cancellationToken));

                builder.MapPatch(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        PermissionUpdateModel model,
                        ICommandHandlerAsync<UserPermissionUpdateCommand> handler,
                        CancellationToken cancellationToken)
                    => await handler.HandleAsync(new UserPermissionUpdateCommand(userGuid, model), cancellationToken));
            }
        }
    }
}