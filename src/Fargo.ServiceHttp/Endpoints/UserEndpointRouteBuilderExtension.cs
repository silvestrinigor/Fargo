using Fargo.Application.Abstractions;
using Fargo.Application.Commands.Users;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos;
using Fargo.Application.Dtos.Users;
using Fargo.Application.Queries.Users;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class UserEndpointRouteBuilderExtension
{
    public static void MapFargoUser(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapUserGroup();

        group.MapGetUser();

        group.MapGetUsers();

        group.MapCreateUser();

        group.MapUpdateUser();

        group.MapSetUserActive();

        group.MapSetUserPermissions();

        group.MapSetUserUserGroups();

        group.MapSetUserPartitions();

        group.MapResetUserPassword();

        group.MapSetUserPasswordExpiration();

        group.MapRemoveUserPasswordExpiration();

        group.MapDeleteUser();
    }

    private static RouteGroupBuilder MapUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
        .MapVersionedGroup("Users", "/users")
        .RequireAuthorization()
        .WithTags("Users")
        .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetUser(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{userGuid:guid}", GetSingleUser)
            .WithName("GetUser")
            .WithSummary("Gets a single user")
            .WithDescription("Retrieves a single user by its unique identifier.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetSingleUser(
        Guid userGuid,
        IQueryHandler<UserSingleQuery, UserDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new UserSingleQuery(userGuid);

        var response = await handler.HandleAsync(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetUsers(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyUser)
            .WithName("GetUsers")
            .WithSummary("Gets multiple users")
            .WithDescription("Retrieves a paginated list of users, including the total number of matching entries.")
            .Produces<PagedResult<UserDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<UserDto>>> GetManyUser(
        int? page,
        int? limit,
        Guid[]? childOfAnyOfThesePartitions,
        Sort? sort,
        string? search,
        IQueryHandler<UsersQuery, PagedResult<UserDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var query = new UsersQuery(
            withPagination,
            childOfAnyOfThesePartitions,
            sort,
            search);

        var response = await handler.HandleAsync(query, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreateUser(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .WithDescription("Creates a new user with optional partitions, user groups, and permissions. Returns the generated identifier. Granting permissions requires ChangeUserPermissions, adding partitions or partition access requires ChangeUserPartitions, and adding user groups requires ChangeUserGroupMembers.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateUser(
        UserCreateDto request,
        ICommandHandler<UserCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserCreateCommand(request);

        var response = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateUser(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Replaces an existing user")
            .WithDescription("Updates the username, names and description of a user. Use the is-active, permissions, user-groups, partitions, password and password-expiration endpoints for the rest.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateUser(
        Guid userGuid,
        UserUpdateDto request,
        ICommandHandler<UserUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserUpdateCommand(userGuid, request);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Set active

    private static IEndpointRouteBuilder MapSetUserActive(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/is-active", SetActive)
            .WithName("SetUserActive")
            .WithSummary("Activates or deactivates a user")
            .WithDescription("Sets whether the user is active. Requires the ActivateUser permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetActive(
        Guid userGuid,
        ActiveStateDto request,
        ICommandHandler<UserSetActiveCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserSetActiveCommand(userGuid, request.IsActive), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set active
    #region Set permissions

    private static IEndpointRouteBuilder MapSetUserPermissions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/permissions", SetPermissions)
            .WithName("SetUserPermissions")
            .WithSummary("Grants and revokes permissions of a user")
            .WithDescription("Adds and removes permissions in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. A caller cannot change its own permissions. Requires the ChangeUserPermissions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPermissions(
        Guid userGuid,
        PermissionsChangeDto request,
        ICommandHandler<UserSetPermissionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserSetPermissionsCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set permissions
    #region Scope and credentials

    private static IEndpointRouteBuilder MapSetUserUserGroups(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/user-groups", SetUserGroups)
            .WithName("SetUserUserGroups")
            .WithSummary("Adds and removes the groups of a user")
            .WithDescription("Adds the user to and removes it from user groups in one request. At least one group is required and none may be in both lists. Joining a group hands the user the permissions of that group and of its ancestors, so the caller must hold all of them. Requires the ChangeUserGroupMembers permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetUserGroups(
        Guid userGuid,
        UserGroupsChangeDto request,
        ICommandHandler<UserSetUserGroupsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserSetUserGroupsCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapSetUserPartitions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/partitions", SetPartitions)
            .WithName("SetUserPartitions")
            .WithSummary("Changes the partitions of a user")
            .WithDescription("Adds and removes the partitions the user belongs to (membership) and the partitions it can access in one request. At least one is required. The caller needs access to every partition it touches. Requires the ChangeUserPartitions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPartitions(
        Guid userGuid,
        PartitionsChangeDto request,
        ICommandHandler<UserSetPartitionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserSetPartitionsCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapResetUserPassword(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/password", ResetPassword)
            .WithName("ResetUserPassword")
            .WithSummary("Resets the password of a user")
            .WithDescription("Sets a new password and requires the user to change it at the next sign-in. A caller cannot reset its own password here; use the identity password change instead. Requires the ChangeAnotherUserPassword permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> ResetPassword(
        Guid userGuid,
        PasswordResetDto request,
        ICommandHandler<UserResetPasswordCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserResetPasswordCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapSetUserPasswordExpiration(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}/password-expiration", SetPasswordExpiration)
            .WithName("SetUserPasswordExpiration")
            .WithSummary("Sets how long the passwords of a user last")
            .WithDescription("Sets the default period after which the password of the user expires. It applies the next time the password is changed; zero means it must be changed immediately. Requires the ChangeAnotherUserPassword permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPasswordExpiration(
        Guid userGuid,
        PasswordExpirationDto request,
        ICommandHandler<UserSetPasswordExpirationCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserSetPasswordExpirationCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    private static IEndpointRouteBuilder MapRemoveUserPasswordExpiration(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{userGuid:guid}/password-expiration", RemovePasswordExpiration)
            .WithName("RemoveUserPasswordExpiration")
            .WithSummary("Removes the password expiration of a user")
            .WithDescription("Clears the default password expiration period, so the passwords of the user no longer expire by default. Requires the ChangeAnotherUserPassword permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> RemovePasswordExpiration(
        Guid userGuid,
        ICommandHandler<UserRemovePasswordExpirationCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new UserRemovePasswordExpirationCommand(userGuid), cancellationToken);

        return TypedResults.NoContent();
    }
    #endregion Scope and credentials

    #region Delete

    private static IEndpointRouteBuilder MapDeleteUser(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{userGuid:guid}", DeleteUser)
            .WithName("DeleteUser")
            .WithSummary("Deletes a user")
            .WithDescription("Deletes the specified user from the system.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeleteUser(
        Guid userGuid,
        ICommandHandler<UserDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new UserDeleteCommand(userGuid);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
