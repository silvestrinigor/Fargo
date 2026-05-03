using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Application.Users;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>Maps all User endpoints.</summary>
public static class UserEndpointRouteBuilderExtension
{
    public static void MapFargoUser(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/users")
            .RequireAuthorization()
            .WithTags("Users");

        group.MapGet("/{userGuid:guid}", GetSingleUser)
            .WithName("GetUser")
            .Produces<UserInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyUsers)
            .WithName("GetUsers")
            .Produces<IReadOnlyCollection<UserInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{userGuid:guid}", UpdateUser)
            .WithName("UpdateUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGuid:guid}", DeleteUser)
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{userGuid:guid}/user-groups/{userGroupGuid:guid}", AddUserGroup)
            .WithName("AddUserGroupToUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGuid:guid}/user-groups/{userGroupGuid:guid}", RemoveUserGroup)
            .WithName("RemoveUserGroupFromUser")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{userGuid:guid}/partitions", GetUserPartitions)
            .WithName("GetUserPartitions")
            .Produces<IReadOnlyCollection<PartitionInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/{userGuid:guid}/partitions/{partitionGuid:guid}", AddUserPartition)
            .WithName("AddUserPartition")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{userGuid:guid}/partitions/{partitionGuid:guid}", RemoveUserPartition)
            .WithName("RemoveUserPartition")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<UserInformation>, NotFound>> GetSingleUser(
        Guid userGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserSingleQuery, UserInformation?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new UserSingleQuery(userGuid, temporalAsOf), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserInformation>>, NoContent>> GetManyUsers(
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

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<Guid>> CreateUser(
        UserCreateModel request,
        ICommandHandler<UserCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new UserCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateUser(
        Guid userGuid,
        UserUpdateModel request,
        ICommandHandler<UserUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserUpdateCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteUser(
        Guid userGuid,
        ICommandHandler<UserDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserDeleteCommand(userGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> AddUserGroup(
        Guid userGuid,
        Guid userGroupGuid,
        ICommandHandler<UserAddUserGroupCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserAddUserGroupCommand(userGuid, userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RemoveUserGroup(
        Guid userGuid,
        Guid userGroupGuid,
        ICommandHandler<UserRemoveUserGroupCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserRemoveUserGroupCommand(userGuid, userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NotFound, NoContent>> GetUserPartitions(
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

        return TypedResults.Ok(result);
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
