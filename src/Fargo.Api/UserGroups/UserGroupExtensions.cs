using Fargo.Application;
using Fargo.Application.UserGroups;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.UserGroups;

public static class UserGroupEndpointRouteBuilderExtension
{
    public static void MapFargoUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapUserGroupGroup();

        group.MapGetUserGroup();

        group.MapGetUserGroups();

        group.MapCreateUserGroup();

        group.MapUpdateUserGroup();

        group.MapDeleteUserGroup();
    }

    private static RouteGroupBuilder MapUserGroupGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/user-groups")
            .RequireAuthorization()
            .WithTags("UserGroups");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{userGroupGuid:guid}", GetSingleUserGroup)
            .WithName("GetUserGroup")
            .WithSummary("Gets a single user group")
            .WithDescription("Retrieves a single user group by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<UserGroupDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<UserGroupDto>, NotFound>> GetSingleUserGroup(
        Guid userGroupGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserGroupSingleQuery, UserGroupDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new UserGroupSingleQuery(userGroupGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetUserGroups(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyUserGroup)
            .WithName("GetUserGroups")
            .WithSummary("Gets multiple user groups")
            .WithDescription("Retrieves a paginated list of user groups. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<UserGroupDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserGroupDto>>, NoContent>> GetManyUserGroup(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions,
        bool? notInsideAnyPartition,
        IQueryHandler<UserGroupsQuery, IReadOnlyCollection<UserGroupDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new UserGroupsQuery(
            withPagination,
            temporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreateUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateUserGroup)
            .WithName("CreateUserGroup")
            .WithSummary("Creates a new user group")
            .WithDescription("Creates a new user group with optional partitions and permissions. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateUserGroup(
        UserGroupCreateCommand request,
        ICommandHandler<UserGroupCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}", UpdateUserGroup)
            .WithName("UpdateUserGroup")
            .WithSummary("Replaces an existing user group")
            .WithDescription("Replaces all user group state including partitions and permissions.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateUserGroup(
        Guid userGroupGuid,
        UserGroupUpdateDto request,
        ICommandHandler<UserGroupUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserGroupUpdateCommand(userGroupGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Delete

    private static IEndpointRouteBuilder MapDeleteUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{userGroupGuid:guid}", DeleteUserGroup)
            .WithName("DeleteUserGroup")
            .WithSummary("Deletes a user group")
            .WithDescription("Deletes the specified user group from the system.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeleteUserGroup(
        Guid userGroupGuid,
        ICommandHandler<UserGroupDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserGroupDeleteCommand(userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
