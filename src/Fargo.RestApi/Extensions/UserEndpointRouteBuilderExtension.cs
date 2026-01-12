using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.HttpApi.Commom;

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
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(new UserSingleQuery(userGuid, atDateTime), cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "/users",
                    async (
                        DateTime? atDateTime,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<UserManyQuery, IEnumerable<UserReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new UserManyQuery(atDateTime, new Pagination(page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPost(
                    "/users",
                    async (
                        UserCreateCommand command,
                        ICommandHandlerAsync<UserCreateCommand, Guid> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.Ok(response);
                    });

                builder.MapDelete(
                    "/users/{userGuid}",
                    async (
                        Guid userGuid,
                        ICommandHandlerAsync<UserDeleteCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new UserDeleteCommand(userGuid), cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapPatch(
                    "/users/{userGuid}",
                    async (
                        Guid userGuid,
                        UserUpdateModel model,
                        ICommandHandlerAsync<UserUpdateCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new UserUpdateCommand(userGuid, model), cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapGet(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        DateTime? atDateTime,
                        IQueryHandlerAsync<UserPermissionAllQuery, IEnumerable<PermissionReadModel>?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var response = await handler.HandleAsync(new UserPermissionAllQuery(userGuid, atDateTime), cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapPatch(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        PermissionUpdateModel model,
                        ICommandHandlerAsync<UserPermissionUpdateCommand> handler,
                        CancellationToken cancellationToken) =>
                    {
                        await handler.HandleAsync(new UserPermissionUpdateCommand(userGuid, model), cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}