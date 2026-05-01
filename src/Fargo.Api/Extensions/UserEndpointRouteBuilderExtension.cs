using Fargo.Api.Contracts;
using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Application.Users;
using Fargo.Domain;
using Fargo.Sdk.Contracts.Partitions;
using Fargo.Sdk.Contracts.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

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
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyUsers)
            .WithName("GetUsers")
            .WithSummary("Gets multiple users")
            .WithDescription("Retrieves a paginated list of users. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<UserDto>>(StatusCodes.Status200OK)
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

        group.MapGet("/{userGuid:guid}/partitions", GetUserPartitions)
            .WithName("GetUserPartitions")
            .WithSummary("Gets the partitions containing a user")
            .WithDescription("Returns the partitions that directly contain the specified user.")
            .Produces<IReadOnlyCollection<PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{userGuid:guid}/partitions/{partitionGuid:guid}", AddUserPartition)
            .WithName("AddUserPartition")
            .WithSummary("Adds a partition to a user")
            .WithDescription("Associates an existing partition with the specified user.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{userGuid:guid}/partitions/{partitionGuid:guid}", RemoveUserPartition)
            .WithName("RemoveUserPartition")
            .WithSummary("Removes a partition from a user")
            .WithDescription("Removes the association between a partition and the specified user.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetSingleUser(
        Guid userGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserSingleQuery, UserInformation?> handler,
        CancellationToken cancellationToken)
    {
        var query = new UserSingleQuery(userGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserDto>>, NoContent>> GetManyUsers(
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        Guid? partitionGuid,
        string? search,
        bool? noPartition,
        IQueryHandler<UserManyQuery, IReadOnlyCollection<UserInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new UserManyQuery(
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            partitionGuid,
            search,
            noPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<UserDto>>(response.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<Ok<Guid>> CreateUser(
        UserCreateRequest request,
        ICommandHandler<UserCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateUser(
        Guid userGuid,
        UserUpdateRequest request,
        ICommandHandler<UserUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(userGuid), cancellationToken);

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

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionDto>>, NotFound, NoContent>> GetUserPartitions(
        Guid userGuid,
        IQueryHandler<UserPartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UserPartitionsQuery(userGuid), cancellationToken);

        if (result is null)
        {
            return TypedResults.NotFound();
        }

        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<PartitionDto>>(result.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<NoContent> AddUserPartition(
        Guid userGuid,
        Guid partitionGuid,
        ICommandHandler<UserAddPartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserAddPartitionCommand(userGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveUserPartition(
        Guid userGuid,
        Guid partitionGuid,
        ICommandHandler<UserRemovePartitionCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserRemovePartitionCommand(userGuid, partitionGuid), cancellationToken);
        return TypedResults.NoContent();
    }
}
