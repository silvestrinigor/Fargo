using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.UserGroups;

using Fargo.Domain;
using Fargo.Domain.Partitions;
using Fargo.Domain.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension responsible for mapping all UserGroup endpoints.
/// </summary>
public static class UserGroupEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all routes related to user groups.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/user-groups")
            .RequireAuthorization()
            .WithTags("UserGroups");

        group.MapGet("/{userGroupGuid:guid}", GetSingleUserGroup)
            .WithName("GetUserGroup")
            .WithSummary("Gets a single user group")
            .WithDescription("Retrieves a single user group by its unique identifier. Supports querying historical data using temporal tables.")
            .Produces<UserGroupInformation>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyUserGroups)
            .WithName("GetUserGroups")
            .WithSummary("Gets multiple user groups")
            .WithDescription("Retrieves a paginated list of user groups. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<UserGroupInformation>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreateUserGroup)
            .WithName("CreateUserGroup")
            .WithSummary("Creates a new user group")
            .WithDescription("Creates a new user group and returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{userGroupGuid:guid}", UpdateUserGroup)
            .WithName("UpdateUserGroup")
            .WithSummary("Updates an existing user group")
            .WithDescription("Updates a user group using partial data.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{userGroupGuid:guid}", DeleteUserGroup)
            .WithName("DeleteUserGroup")
            .WithSummary("Deletes a user group")
            .WithDescription("Deletes the specified user group from the system.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{userGroupGuid:guid}/partitions", GetUserGroupPartitions)
            .WithName("GetUserGroupPartitions")
            .WithSummary("Gets the partitions containing a user group")
            .WithDescription("Returns the partitions that directly contain the specified user group.")
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
        var query = new UserGroupSingleQuery(userGroupGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return TypedResultsHelpers.HandleQueryResult(response);
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

        return TypedResultsHelpers.HandleCollectionQueryResult(response);
    }

    private static async Task<Ok<Guid>> CreateUserGroup(
        UserGroupCreateCommand command,
        ICommandHandler<UserGroupCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdateUserGroup(
        Guid userGroupGuid,
        UserGroupUpdateModel model,
        ICommandHandler<UserGroupUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserGroupUpdateCommand(userGroupGuid, model);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeleteUserGroup(
        Guid userGroupGuid,
        ICommandHandler<UserGroupDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserGroupDeleteCommand(userGroupGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionInformation>>, NotFound, NoContent>> GetUserGroupPartitions(
        Guid userGroupGuid,
        IQueryHandler<UserGroupPartitionsQuery, IReadOnlyCollection<PartitionInformation>?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new UserGroupPartitionsQuery(userGroupGuid), cancellationToken);

        return TypedResultsHelpers.HandleNullableCollectionQueryResult(result);
    }
}
