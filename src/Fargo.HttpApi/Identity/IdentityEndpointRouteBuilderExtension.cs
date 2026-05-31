using Fargo.Application;
using Fargo.Application.Identity;
using Fargo.Application.Shared.Identity;
using Fargo.Application.Shared.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.HttpApi.Identity;

/// <summary>
/// Extension responsible for mapping identity endpoints.
/// </summary>
public static class IdentityEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all identity routes.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoIdentity(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/identity")
            .WithTags("Identity");

        group.MapPost("/login", Login)
            .WithName("Login")
            .WithSummary("Authenticates a user")
            .WithDescription("Validates user credentials and returns an access token and refresh token.")
            .Produces<AuthResult>(StatusCodes.Status200OK)
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
            .Produces<AuthResult>(StatusCodes.Status200OK)
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

    private static async Task<Ok<AuthResult>> Login(
        LoginCommand request,
        ICommandHandler<LoginCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok> Logout(
        LogoutCommand request,
        ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request, cancellationToken);

        return TypedResults.Ok();
    }

    private static async Task<Ok<AuthResult>> Refresh(
        RefreshCommand request,
        ICommandHandler<RefreshCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> ChangePassword(
        UserPasswordUpdateDto request,
        ICommandHandler<PasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PasswordChangeCommand(request), cancellationToken);

        return TypedResults.NoContent();
    }
}
