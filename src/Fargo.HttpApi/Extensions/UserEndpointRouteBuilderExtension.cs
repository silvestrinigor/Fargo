using Fargo.Application.Models.UserModels;
using Fargo.Application.Requests.Commands;
using Fargo.Application.Requests.Commands.UserCommands;
using Fargo.Application.Requests.Queries;
using Fargo.Application.Requests.Queries.UserQueries;
using Fargo.Domain.ValueObjects;
using Fargo.HttpApi.Helpers;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Extensions;

/// <summary>
/// Extension responsible for mapping all User endpoints.
/// </summary>
public static class UserEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all routes related to users.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoUser(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/users")
            .RequireAuthorization()
            .WithTags("Users");

        group.MapGet("/{userGuid:guid}", GetSingleUser)
            .WithName("GetUser")
            .WithSummary("Gets a single user")
            .WithDescription("Retrieves a single user by its unique identifier. Supports querying historical data using temporal tables.")
            .Produces<UserInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyUsers)
            .WithName("GetUsers")
            .WithSummary("Gets multiple users")
            .WithDescription("Retrieves a paginated list of users. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<UserInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .WithDescription("Creates a new user and returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{userGuid:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Updates an existing user")
            .WithDescription("Updates a user using partial data.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGuid:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Deletes a user")
            .WithDescription("Deletes the specified user from the system.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{userGuid:guid}/user-groups/{userGroupGuid:guid}", AddUserGroup)
            .WithName("AddUserGroupToUser")
            .WithSummary("Adds a user group to a user")
            .WithDescription("Associates an existing user group with the specified user.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGuid:guid}/user-groups/{userGroupGuid:guid}", RemoveUserGroup)
            .WithName("RemoveUserGroupFromUser")
            .WithSummary("Removes a user group from a user")
            .WithDescription("Removes the association between an existing user group and the specified user.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<UserInformation>, NotFound>> GetSingleUser(
        Guid userGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserSingleQuery, UserInformation?> handler,
        CancellationToken cancellationToken)
    {
        var query = new UserSingleQuery(userGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return TypedResultsHelpers.HandleQueryResult(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserInformation>>, NoContent>> GetManyUsers(
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new UserManyQuery(
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit)
        );

        var response = await handler.Handle(query, cancellationToken);

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
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

    private static async Task<NoContent> AddUserGroup(
        Guid userGuid,
        Guid userGroupGuid,
        ICommandHandler<UserAddUserGroupCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserAddUserGroupCommand(userGuid, userGroupGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveUserGroup(
        Guid userGuid,
        Guid userGroupGuid,
        ICommandHandler<UserRemoveUserGroupCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserRemoveUserGroupCommand(userGuid, userGroupGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
