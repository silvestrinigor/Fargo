using Fargo.Application;
using Fargo.Application.Users;
using Fargo.HttpApi.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Users;

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
            .Produces<HttpContracts.UserDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<HttpContracts.UserDto>, NotFound>> GetSingleUser(
        Guid userGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<UserSingleQuery, UserDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new UserSingleQuery(userGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetUsers(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyUser)
            .WithName("GetUsers")
            .WithSummary("Gets multiple users")
            .WithDescription("Retrieves a paginated list of users. Supports optional temporal queries and partition filters, including public users without partitions.")
            .Produces<IReadOnlyCollection<HttpContracts.UserDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<HttpContracts.UserDto>>, NoContent>> GetManyUser(
        DateTimeOffset? temporalAsOfDateTime,
        int? page,
        int? limit,
        [FromQuery] Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<UsersQuery, IReadOnlyCollection<UserDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var query = new UsersQuery(
            withPagination,
            temporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        IReadOnlyCollection<HttpContracts.UserDto> contractResponse =
            response.Select(static user => user.ToContract()).ToArray();

        return TypedResults.Ok(contractResponse);
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
        HttpContracts.UserCreateRequest request,
        ICommandHandler<UserCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new UserCreateCommand(request.ToApplication()), cancellationToken);

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
        HttpContracts.UserUpdateRequest request,
        ICommandHandler<UserUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new UserUpdateCommand(userGuid, request.ToApplication()), cancellationToken);

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
