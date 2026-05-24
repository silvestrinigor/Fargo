using Fargo.Application;
using Fargo.Application.Identity;
using Fargo.Application.Users;
using Fargo.Core.Identity;
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
        LoginRequest request,
        ICommandHandler<LoginCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new LoginCommand(request.Nameid, request.Password), cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<Ok> Logout(
        RefreshRequest request,
        ICommandHandler<LogoutCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new LogoutCommand(new Token(request.RefreshToken)), cancellationToken);

        return TypedResults.Ok();
    }

    private static async Task<Ok<AuthResult>> Refresh(
        RefreshRequest request,
        ICommandHandler<RefreshCommand, AuthResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new RefreshCommand(new Token(request.RefreshToken)), cancellationToken);

        return TypedResults.Ok(result);
    }

    private static async Task<NoContent> ChangePassword(
        PasswordUpdateRequest request,
        ICommandHandler<PasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PasswordChangeCommand(new UserPasswordUpdateDto(request.NewPassword, request.CurrentPassword)), cancellationToken);

        return TypedResults.NoContent();
    }
}

public sealed record LoginRequest(string Nameid, string Password);

public sealed record RefreshRequest(string RefreshToken);

public sealed record PasswordUpdateRequest(string NewPassword, string CurrentPassword);
