using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Identity;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Http.Endpoints;

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
        ICommandHandler<IdentityLoginCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityLoginCommand(request.Nameid, request.Password);

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
        ICommandHandler<IdentityLogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityLogoutCommand(request.RefreshToken);

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
        ICommandHandler<IdentityRefreshCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityRefreshCommand(request.RefreshToken);

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    #endregion

    #region Change password

    private static IEndpointRouteBuilder MapIdentityChangePassword(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/password", ChangePassword)
            .WithName("ChangePassword")
            .WithSummary("Changes the password of the authenticated user")
            .WithDescription("Validates the current password and updates it with the new password.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);

        return builder;
    }

    private static async Task<NoContent> ChangePassword(
        PasswordUpdateDto request,
        ICommandHandler<IdentityPasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityPasswordChangeCommand(request);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion
}
