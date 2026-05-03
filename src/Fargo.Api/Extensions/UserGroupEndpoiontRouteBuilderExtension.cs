using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Application.UserGroups;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>Maps all UserGroup endpoints.</summary>
public static class UserGroupEndpointRouteBuilderExtension
{
    public static void MapFargoUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/user-groups")
            .RequireAuthorization()
            .WithTags("UserGroups");

        group.MapGet("/{userGroupGuid:guid}", GetSingleUserGroup)
            .WithName("GetUserGroup")
            .Produces<UserGroupInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyUserGroups)
            .WithName("GetUserGroups")
            .Produces<IReadOnlyCollection<UserGroupInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateUserGroup)
            .WithName("CreateUserGroup")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{userGroupGuid:guid}", UpdateUserGroup)
            .WithName("UpdateUserGroup")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGroupGuid:guid}", DeleteUserGroup)
            .WithName("DeleteUserGroup")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{userGroupGuid:guid}/partitions", GetUserGroupPartitions)
            .WithName("GetUserGroupPartitions")
            .Produces<IReadOnlyCollection<PartitionInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<UserGroupInformation>, NotFound>> GetSingleUserGroup(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserGroupSingleQuery, UserGroupInformation?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new UserGroupSingleQuery(userGroupGuid, temporalAsOf), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserGroupInformation>>, NoContent>> GetManyUserGroups(
        Guid? userGuid,
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        IQueryHandler<UserGroupManyQuery, IReadOnlyCollection<UserGroupInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new UserGroupManyQuery(
            userGuid,
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit)
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    private static async Task<Ok<Guid>> CreateUserGroup(
        UserGroupCreateModel request,
        ICommandHandler<UserGroupCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new UserGroupCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateUserGroup(
        Guid userGroupGuid,
        UserGroupUpdateModel request,
        ICommandHandler<UserGroupUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserGroupUpdateCommand(userGroupGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteUserGroup(
        Guid userGroupGuid,
        ICommandHandler<UserGroupDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserGroupDeleteCommand(userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NotFound, NoContent>> GetUserGroupPartitions(
        Guid userGroupGuid,
        IQueryHandler<UserGroupPartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UserGroupPartitionsQuery(userGroupGuid), cancellationToken);

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
}
