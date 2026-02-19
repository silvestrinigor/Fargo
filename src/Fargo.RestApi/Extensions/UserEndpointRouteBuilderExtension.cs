using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.HttpApi.Helpers;

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
                            IQueryHandler<UserSingleQuery, UserReadModel?> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new UserSingleQuery(userGuid, temporalAsOf);

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                builder.MapGet(
                        "/users",
                        async (
                            DateTime? temporalAsOf,
                            Page? page,
                            Limit? limit,
                            IQueryHandler<UserManyQuery, IEnumerable<UserReadModel>> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new UserManyQuery(temporalAsOf, new(page ?? default, limit ?? default));

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                builder.MapPost(
                        "/users",
                        async (
                            UserCreateCommand command,
                            ICommandHandler<UserCreateCommand, Guid> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var response = await handler.Handle(command, cancellationToken);

                        return TypedResults.Ok(response);
                        });

                builder.MapDelete(
                        "/users/{userGuid}",
                        async (
                            Guid userGuid,
                            ICommandHandler<UserDeleteCommand> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new UserDeleteCommand(userGuid);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });

                builder.MapPatch(
                        "/users/{userGuid}",
                        async (
                            Guid userGuid,
                            UserUpdateModel model,
                            ICommandHandler<UserUpdateCommand> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new UserUpdateCommand(userGuid, model);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });

                builder.MapGet(
                        "/users/{userGuid}/permissions",
                        async (
                            Guid userGuid,
                            DateTime? temporalAsOf,
                            Limit? limit,
                            Page? page,
                            IQueryHandler<UserPermissionManyQuery, IEnumerable<PermissionReadModel>?> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var query = new UserPermissionManyQuery(
                                userGuid,
                                temporalAsOf,
                                new(page ?? default, limit ?? default)
                                );

                        var response = await handler.Handle(query, cancellationToken);

                        return TypedResultsHelpers.HandleQueryResult(response);
                        });

                builder.MapPatch(
                        "/users/{userGuid}/permissions",
                        async (
                            Guid userGuid,
                            PermissionUpdateModel model,
                            ICommandHandler<UserPermissionUpdateCommand> handler,
                            CancellationToken cancellationToken) =>
                        {
                        var command = new UserPermissionUpdateCommand(userGuid, model);

                        await handler.Handle(command, cancellationToken);

                        return TypedResults.NoContent();
                        });
            }
        }
    }
}