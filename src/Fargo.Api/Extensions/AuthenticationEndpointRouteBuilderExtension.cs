using Fargo.Api.Contracts;
using Fargo.Api.Contracts.Authentication;
using Fargo.Application;
using Fargo.Application.Authentication;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension responsible for mapping authentication endpoints.
/// </summary>
public static class AuthenticationEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all authentication routes.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoAuthentication(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/authentication")
            .WithTags("Authentication");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticates a user")
            .WithDescription("Validates user credentials and returns an access token and refresh token.")
            .Produces<AuthDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/logout", Logout)
            .WithName("Logout")
            .WithSummary("Logs out the current user")
            .WithDescription("Invalidates the current refresh token or session.")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPost("/refresh", Refresh)
            .WithName("RefreshToken")
            .WithSummary("Refreshes the access token")
            .WithDescription("Uses a valid refresh token to generate a new access token.")
            .Produces<AuthDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        group.MapPut("/password", ChangePassword)
            .RequireAuthorization()
            .WithName("ChangePassword")
            .WithSummary("Changes the password of the authenticated user")
            .WithDescription("Validates the current password and updates it with the new password.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);
    }

    private static async Task<Ok<AuthDto>> Login(
        LoginDto request,
        ICommandHandler<LoginCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.Ok(result.ToContract());
    }

    private static async Task<Ok> Logout(
        RefreshDto request,
        ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToLogoutCommand(), cancellationToken);

        return TypedResults.Ok();
    }

    private static async Task<Ok<AuthDto>> Refresh(
        RefreshDto request,
        ICommandHandler<RefreshCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToRefreshCommand(), cancellationToken);

        return TypedResults.Ok(result.ToContract());
    }

    private static async Task<NoContent> ChangePassword(
        PasswordUpdateDto request,
        ICommandHandler<PasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.NoContent();
    }
}
