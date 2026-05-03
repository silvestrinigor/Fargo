using Fargo.Application;
using Fargo.Application.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Users;

public static class UserEndpointRouteBuilderExtension
{
    public static void MapFargoUser(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapUserGroup();

        group.MapGetUser();

        group.MapGetUsers();

        group.MapCreateUser();

        group.MapUpdateUser();

        group.MapDeleteUser();
    }

    private static RouteGroupBuilder MapUserGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/users")
            .RequireAuthorization()
            .WithTags("Users");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetUser(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{userGuid:guid}", GetSingleUser)
            .WithName("GetUser")
            .WithSummary("Gets a single user")
            .WithDescription("Retrieves a single user by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<UserDto>, NotFound>> GetSingleUser(
        Guid userGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserSingleQuery, UserDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new UserSingleQuery(userGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetUsers(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyUser)
            .WithName("GetUsers")
            .WithSummary("Gets multiple users")
            .WithDescription("Retrieves a paginated list of users. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<UserDto>>, NoContent>> GetManyUser(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions,
        bool? notInsideAnyPartition,
        IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new UsersQuery(
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

    private static IEndpointRouteBuilder MapCreateUser(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreateUser)
            .WithName("CreateUser")
            .WithSummary("Creates a new user")
            .WithDescription("Creates a new user with optional partitions, user groups, and permissions. Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreateUser(
        UserCreateCommand request,
        ICommandHandler<UserCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdateUser(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{userGuid:guid}", UpdateUser)
            .WithName("UpdateUser")
            .WithSummary("Replaces an existing user")
            .WithDescription("Replaces all user state including partitions, user groups, and permissions.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdateUser(
        Guid userGuid,
        UserUpdateDto request,
        ICommandHandler<UserUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserUpdateCommand(userGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

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
        await handler.Handle(new UserDeleteCommand(userGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
