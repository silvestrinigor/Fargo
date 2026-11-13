using Fargo.Application.Abstractions;
using Fargo.Application.Commands.UserGroups;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos;
using Fargo.Application.Dtos.UserGroups;
using Fargo.Application.Queries.UserGroups;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class UserGroupEndpointRouteBuilderExtension
{
    public static void MapFargoUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapUserGroupGroup();

        group.MapGetUserGroupByGuid();

        group.MapGetUserGroups();

        group.MapCreateUserGroup();

        group.MapUpdateUserGroup();

        group.MapSetUserGroupActive();

        group.MapSetUserGroupPermissions();

        group.MapSetUserGroupPartitions();

        group.MapSetUserGroupParent();

        group.MapRemoveUserGroupParent();

        group.MapDeleteUserGroup();
    }

    private static RouteGroupBuilder MapUserGroupGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("UserGroups", "/user-groups")
        .RequireAuthorization()
        .WithTags("UserGroups")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetUserGroupByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{userGroupGuid:guid}", GetSingleUserGroup)
            .WithName("GetUserGroup")
            .WithSummary("Gets a single user group")
            .WithDescription("Retrieves a single user group by its unique identifier.")
            .Produces<UserGroupDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<UserGroupDto>, NotFound>> GetSingleUserGroup(
        Guid userGroupGuid,
        IQueryHandler<UserGroupSingleQuery, UserGroupDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new UserGroupSingleQuery(userGroupGuid);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetUserGroups(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyUserGroup)
            .WithName("GetUserGroups")
            .WithSummary("Gets multiple user groups")
            .WithDescription("Retrieves a paginated list of user groups, including the total number of matching entries.")
            .Produces<PagedResult<UserGroupDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<UserGroupDto>>> GetManyUserGroup(
        int? page,
        int? limit,
        Guid[]? childOfAnyOfThesePartitions,
        Sort? sort,
        string? search,
        IQueryHandler<UserGroupsQuery, PagedResult<UserGroupDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var query = new UserGroupsQuery(
            withPagination,
            childOfAnyOfThesePartitions,
            sort,
            search);

        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreateUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateUserGroup)
            .WithName("CreateUserGroup")
            .WithSummary("Creates a new user group")
            .WithDescription("Creates a new user group with optional partitions and permissions. Returns the generated identifier. Granting permissions requires ChangeUserGroupPermissions, adding partitions or partition access requires ChangeUserGroupPartitions, and setting a parent requires ChangeUserGroupParent.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateUserGroup(
        UserGroupCreateDto request,
        ICommandHandler<UserGroupCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new UserGroupCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateUserGroup(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}", UpdateUserGroup)
            .WithName("UpdateUserGroup")
            .WithSummary("Replaces an existing user group")
            .WithDescription("Updates the name and description of a user group. Use the is-active, permissions, partitions and parent endpoints for the rest.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateUserGroup(
        Guid userGroupGuid,
        UserGroupUpdateDto request,
        ICommandHandler<UserGroupUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupUpdateCommand(userGroupGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Set active

    private static IEndpointRouteBuilder MapSetUserGroupActive(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}/is-active", SetActive)
            .WithName("SetUserGroupActive")
            .WithSummary("Activates or deactivates a user group")
            .WithDescription("Sets whether the user group is active. Requires the ActivateUserGroup permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetActive(
        Guid userGroupGuid,
        ActiveStateDto request,
        ICommandHandler<UserGroupSetActiveCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupSetActiveCommand(userGroupGuid, request.IsActive), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set active
    #region Set permissions

    private static IEndpointRouteBuilder MapSetUserGroupPermissions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}/permissions", SetPermissions)
            .WithName("SetUserGroupPermissions")
            .WithSummary("Grants and revokes permissions of a user group")
            .WithDescription("Adds and removes permissions in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. Requires the ChangeUserGroupPermissions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPermissions(
        Guid userGroupGuid,
        PermissionsChangeDto request,
        ICommandHandler<UserGroupSetPermissionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupSetPermissionsCommand(userGroupGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set permissions
    #region Scope and hierarchy

    private static IEndpointRouteBuilder MapSetUserGroupPartitions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}/partitions", SetPartitions)
            .WithName("SetUserGroupPartitions")
            .WithSummary("Changes the partitions of a user group")
            .WithDescription("Adds and removes the partitions the group belongs to (membership) and the partitions it can access in one request. At least one is required. The caller needs access to every partition it touches. Requires the ChangeUserGroupPartitions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPartitions(
        Guid userGroupGuid,
        PartitionsChangeDto request,
        ICommandHandler<UserGroupSetPartitionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupSetPartitionsCommand(userGroupGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapSetUserGroupParent(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGroupGuid:guid}/parent", SetParent)
            .WithName("SetUserGroupParent")
            .WithSummary("Places a user group under a parent")
            .WithDescription("Sets the parent of the group. Members of the group inherit the permissions of the parent and of its ancestors, so the caller must hold all of them. Rejects an assignment that would create a cycle. Setting the parent the group already has does nothing. Requires the ChangeUserGroupParent permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetParent(
        Guid userGroupGuid,
        ParentUserGroupDto request,
        ICommandHandler<UserGroupSetParentCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupSetParentCommand(userGroupGuid, request.ParentUserGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapRemoveUserGroupParent(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{userGroupGuid:guid}/parent", RemoveParent)
            .WithName("RemoveUserGroupParent")
            .WithSummary("Removes the parent of a user group")
            .WithDescription("Detaches the group from its parent, so its members stop inheriting the permissions of the parent. A group without a parent is left as it is. Requires the ChangeUserGroupParent permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> RemoveParent(
        Guid userGroupGuid,
        ICommandHandler<UserGroupRemoveParentCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserGroupRemoveParentCommand(userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }
    #endregion Scope and hierarchy

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
        await handler.HandleAsync(new UserGroupDeleteCommand(userGroupGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
