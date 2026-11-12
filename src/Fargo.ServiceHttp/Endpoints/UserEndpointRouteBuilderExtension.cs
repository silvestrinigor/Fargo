using Fargo.Application.Abstractions;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos;
using Fargo.Application.Commands.Users;
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
            .WithDescription("Creates a new user with optional partitions, user groups, and permissions. Returns the generated identifier. Granting permissions requires the ChangeUserPermissions permission.")
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
            .WithDescription("Updates user state: names, description, authentication, partitions and user groups. Use the is-active and permissions endpoints to change those.")
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
