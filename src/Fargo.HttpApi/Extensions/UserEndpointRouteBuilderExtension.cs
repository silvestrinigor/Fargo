using Fargo.Application.Commom;
using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions
{
    public static class UserEndpointRouteBuilderExtension
    {
        public static void MapFargoUser(this IEndpointRouteBuilder builder)
        {
            var group = builder
                .MapGroup("/users")
                .RequireAuthorization();

            group.MapGet("/{userGuid:guid}", GetSingleUser);
            group.MapGet("/", GetManyUsers);
            group.MapPost("/", CreateUser);
            group.MapPatch("/{userGuid:guid}", UpdateUser);
            group.MapDelete("/{userGuid:guid}", DeleteUser);
        }

        private static async Task<Results<Ok<UserResponseModel>, NotFound>> GetSingleUser(
            Guid userGuid,
            DateTime? temporalAsOf,
            IQueryHandler<UserSingleQuery, UserResponseModel?> handler,
            CancellationToken cancellationToken)
        {
            var query = new UserSingleQuery(userGuid, temporalAsOf);

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Results<Ok<IEnumerable<UserResponseModel>>, NotFound, NoContent>> GetManyUsers(
            DateTime? temporalAsOf,
            Page? page,
            Limit? limit,
            IQueryHandler<UserManyQuery, IEnumerable<UserResponseModel>> handler,
            CancellationToken cancellationToken)
        {
            var query = new UserManyQuery(
                temporalAsOf,
                new(page ?? default, limit ?? default)
            );

            var response = await handler.Handle(query, cancellationToken);

            return TypedResultsHelpers.HandleQueryResult(response);
        }

        private static async Task<Ok<Guid>> CreateUser(
            UserCreateCommand command,
            ICommandHandler<UserCreateCommand, Guid> handler,
            CancellationToken cancellationToken)
        {
            var response = await handler.Handle(command, cancellationToken);

            return TypedResults.Ok(response);
        }

        private static async Task<NoContent> UpdateUser(
            Guid userGuid,
            UserUpdateModel model,
            ICommandHandler<UserUpdateCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new UserUpdateCommand(userGuid, model);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }

        private static async Task<NoContent> DeleteUser(
            Guid userGuid,
            ICommandHandler<UserDeleteCommand> handler,
            CancellationToken cancellationToken)
        {
            var command = new UserDeleteCommand(userGuid);

            await handler.Handle(command, cancellationToken);

            return TypedResults.NoContent();
        }
    }
}