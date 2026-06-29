using Fargo.Application;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Identity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Endpoints;

public static class IdentityEndpointRouteBuilderExtension
{
    public static void MapFargoIdentity(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapIdentityGroup();

        group.MapIdentityLogin();

        group.MapIdentityLogout();

        group.MapIdentityRefresh();

        group.MapIdentityChangePassword();
    }

    private static RouteGroupBuilder MapIdentityGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity");

        return group;
    }

    #region Login

    private static IEndpointRouteBuilder MapIdentityLogin(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticates a user")
            .WithDescription("Validates user credentials and returns an access token and refresh token.")
            .Produces<AuthResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<Ok<AuthResult>> Login(
        LoginDto request,
        ICommandHandler<LoginCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new LoginCommand(request.Nameid, request.Password);

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    #endregion

    #region Logout

    private static IEndpointRouteBuilder MapIdentityLogout(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logs out the current user")
            .WithDescription("Invalidates the current refresh token or session.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<Ok> Logout(
        LogOutDto request,
        ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand(request.RefreshToken);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok();
    }

    #endregion

    #region Refresh

    private static IEndpointRouteBuilder MapIdentityRefresh(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refreshes the access token")
            .WithDescription("Uses a valid refresh token to generate a new access token.")
            .Produces<AuthResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<Ok<AuthResult>> Refresh(
        RefreshDto request,
        ICommandHandler<RefreshCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new RefreshCommand(request.RefreshToken);

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    #endregion

    #region Change password

    private static IEndpointRouteBuilder MapIdentityChangePassword(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/password", ChangePassword)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Changes the password of the authenticated user")
            .WithDescription("Validates the current password and updates it with the new password.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<NoContent> ChangePassword(
        IdentityPasswordUpdateDto request,
        ICommandHandler<PasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new PasswordChangeCommand(request);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion
}
