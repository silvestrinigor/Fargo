using Fargo.Application.Commom;
using Fargo.Application.Mediators;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands;
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
                        DateTime? temporalAsOf,
                        IQueryHandlerAsync<UserSingleQuery, UserReadModel?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new UserSingleQuery(userGuid, temporalAsOf);

                        var response = await handler.HandleAsync(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                    });

                builder.MapGet(
                    "/users",
                    async (
                        DateTime? temporalAsOf,
                        Page? page,
                        Limit? limit,
                        IQueryHandlerAsync<UserManyQuery, IEnumerable<UserReadModel>> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new UserManyQuery(temporalAsOf, new (page ?? default, limit ?? default));

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
                        var command = new UserDeleteCommand(userGuid);

                        await handler.HandleAsync(command, cancellationToken);

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
                        var command = new UserUpdateCommand(userGuid, model);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });

                builder.MapGet(
                    "/users/{userGuid}/permissions",
                    async (
                        Guid userGuid,
                        DateTime? temporalAsOf,
                        Limit? limit,
                        Page? page,
                        IQueryHandlerAsync<UserPermissionManyQuery, IEnumerable<PermissionReadModel>?> handler,
                        CancellationToken cancellationToken) =>
                    {
                        var query = new UserPermissionManyQuery(userGuid, temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.HandleAsync(query, cancellationToken);

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
                        var command = new UserPermissionUpdateCommand(userGuid, model);

                        await handler.HandleAsync(command, cancellationToken);

                        return TypedResults.NoContent();
                    });
            }
        }
    }
}