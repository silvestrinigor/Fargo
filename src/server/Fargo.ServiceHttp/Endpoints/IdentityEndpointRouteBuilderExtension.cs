using Fargo.Application.Common;
using Fargo.Application.Identity;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

public static class IdentityEndpointRouteBuilderExtension
{
    public static void MapFargoIdentity(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapIdentityGroup();

        group.MapIdentityLogin();

        group.MapIdentityLogout();

        group.MapIdentityRefresh();

        group.MapIdentityChangePassword();

        group.MapIdentityServiceToken();
    }

    private static RouteGroupBuilder MapIdentityGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapVersionedGroup("Identity", "/identity")
            .WithTags("Identity");

        return group;
    }

    #region Login

    private static IEndpointRouteBuilder MapIdentityLogin(this IEndpointRouteBuilder builder)
    {
        builder
        .MapPost("/login", Login)
        .WithName("Login")
        .WithSummary("Authenticates a user")
        .WithDescription("Validates user credentials and returns an access token and refresh token.")
        .Produces<IdentityAuthResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .RequireRateLimiting("identity-login");

        return builder;
    }

    private static async Task<Ok<IdentityAuthResultDto>> Login(
        IdentityLoginDto request,
        ICommandHandler<IdentityLoginCommand, IdentityAuthResultDto> handler,
        HttpContext httpContext,
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
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("identity-default");

        return builder;
    }

    private static async Task<Ok> Logout(
        IdentityLogOutDto request,
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
            .Produces<IdentityAuthResultDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("identity-refresh");

        return builder;
    }

    private static async Task<Ok<IdentityAuthResultDto>> Refresh(
        IdentityRefreshDto request,
        ICommandHandler<IdentityRefreshCommand, IdentityAuthResultDto> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityRefreshCommand(request.RefreshToken);

        var result = await handler.HandleAsync(command, cancellationToken);

        return TypedResults.Ok(result);
    }

    #endregion

    #region Service token

    private static IEndpointRouteBuilder MapIdentityServiceToken(this IEndpointRouteBuilder builder)
    {
        builder
        .MapPost("/service-token", ServiceToken)
        .WithName("ServiceToken")
        .WithSummary("Authenticates a service account")
        .WithDescription("Validates service-account client credentials (client id + secret) and returns a short-lived access token. Services re-exchange their credentials for a new token when the current one nears expiry; no refresh token is issued.")
        .Produces<ServiceTokenResultDto>(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status401Unauthorized)
        .Produces(StatusCodes.Status429TooManyRequests)
        .RequireRateLimiting("identity-login");

        return builder;
    }

    private static async Task<Ok<ServiceTokenResultDto>> ServiceToken(
        ServiceTokenRequestDto request,
        ICommandHandler<ServiceTokenCommand, ServiceTokenResultDto> handler,
        CancellationToken cancellationToken)
    {
        var command = new ServiceTokenCommand(request.ClientId, request.ClientSecret);

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
            .Produces(StatusCodes.Status401Unauthorized)
            .RequireRateLimiting("identity-default");

        return builder;
    }

    private static async Task<NoContent> ChangePassword(
        IdentityPasswordUpdateDto request,
        ICommandHandler<IdentityPasswordChangeCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new IdentityPasswordChangeCommand(request);

        await handler.HandleAsync(command, cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion
}
