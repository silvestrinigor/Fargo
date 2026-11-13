using Fargo.Application.Abstractions;
using Fargo.Application.Commands.ServiceAccounts;
using Fargo.Application.Common.Paging;
using Fargo.Application.Dtos;
using Fargo.Application.Dtos.ServiceAccounts;
using Fargo.Application.Queries.ServiceAccounts;
using Fargo.ServiceHttp.Versioning;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.ServiceHttp.Endpoints;

/// <summary>
/// Admin-facing CRUD, separate from the token endpoint under /identity; each operation is gated by a
/// service-account permission.
/// </summary>
public static class ServiceAccountEndpointRouteBuilderExtension
{
    public static void MapFargoServiceAccount(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapServiceAccountGroup();

        group.MapGetServiceAccountByGuid();
        group.MapGetServiceAccounts();
        group.MapCreateServiceAccount();
        group.MapUpdateServiceAccount();

        group.MapSetServiceAccountActive();

        group.MapSetServiceAccountPermissions();

        group.MapSetServiceAccountPartitions();
        group.MapRotateServiceAccountSecret();
        group.MapDeleteServiceAccount();
    }

    private static RouteGroupBuilder MapServiceAccountGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapVersionedGroup("ServiceAccounts", "/service-accounts")
            .RequireAuthorization()
            .WithTags("ServiceAccounts")
            .RequireRateLimiting("general-authenticated");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetServiceAccountByGuid(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{serviceAccountGuid:guid}", GetSingle)
            .WithName("GetServiceAccount")
            .WithSummary("Gets a single service account")
            .WithDescription("Retrieves a single service account by its unique identifier. Never returns the secret.")
            .Produces<ServiceAccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Results<Ok<ServiceAccountDto>, NotFound>> GetSingle(
        Guid serviceAccountGuid,
        IQueryHandler<ServiceAccountSingleQuery, ServiceAccountDto?> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new ServiceAccountSingleQuery(serviceAccountGuid), cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion

    #region Get Many

    private static IEndpointRouteBuilder MapGetServiceAccounts(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetMany)
            .WithName("GetServiceAccounts")
            .WithSummary("Gets multiple service accounts")
            .WithDescription("Retrieves a paginated list of service accounts, including the total number of matching entries.")
            .Produces<PagedResult<ServiceAccountDto>>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<PagedResult<ServiceAccountDto>>> GetMany(
        int? page,
        int? limit,
        IQueryHandler<ServiceAccountsQuery, PagedResult<ServiceAccountDto>> handler,
        CancellationToken cancellationToken)
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var response = await handler.HandleAsync(new ServiceAccountsQuery(withPagination), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion

    #region Create

    private static IEndpointRouteBuilder MapCreateServiceAccount(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", Create)
            .WithName("CreateServiceAccount")
            .WithSummary("Creates a new service account")
            .WithDescription("Creates a service account and returns its generated client secret EXACTLY ONCE. Store the secret securely; it cannot be retrieved later (only rotated). Granting permissions requires ChangeServiceAccountPermissions, and adding partitions or partition access requires ChangeServiceAccountPartitions.")
            .Produces<ServiceAccountCreatedDto>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<ServiceAccountCreatedDto>> Create(
        ServiceAccountCreateDto request,
        ICommandHandler<ServiceAccountCreateCommand, ServiceAccountCreatedDto> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new ServiceAccountCreateCommand(request), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion

    #region Update

    private static IEndpointRouteBuilder MapUpdateServiceAccount(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{serviceAccountGuid:guid}", Update)
            .WithName("UpdateServiceAccount")
            .WithSummary("Updates a service account")
            .WithDescription("Updates the description of a service account. Use the is-active, permissions and partitions endpoints for the rest.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> Update(
        Guid serviceAccountGuid,
        ServiceAccountUpdateDto request,
        ICommandHandler<ServiceAccountUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ServiceAccountUpdateCommand(serviceAccountGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion

    #region Set active

    private static IEndpointRouteBuilder MapSetServiceAccountActive(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{serviceAccountGuid:guid}/is-active", SetActive)
            .WithName("SetServiceAccountActive")
            .WithSummary("Activates or deactivates a service account")
            .WithDescription("Sets whether the service account is active. Requires the ActivateServiceAccount permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetActive(
        Guid serviceAccountGuid,
        ActiveStateDto request,
        ICommandHandler<ServiceAccountSetActiveCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ServiceAccountSetActiveCommand(serviceAccountGuid, request.IsActive), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set active
    #region Set permissions

    private static IEndpointRouteBuilder MapSetServiceAccountPermissions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{serviceAccountGuid:guid}/permissions", SetPermissions)
            .WithName("SetServiceAccountPermissions")
            .WithSummary("Grants and revokes permissions of a service account")
            .WithDescription("Adds and removes permissions in one request. At least one permission is required and none may be in both lists. The caller must hold every permission it grants or revokes. A caller cannot change its own permissions. Requires the ChangeServiceAccountPermissions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPermissions(
        Guid serviceAccountGuid,
        PermissionsChangeDto request,
        ICommandHandler<ServiceAccountSetPermissionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ServiceAccountSetPermissionsCommand(serviceAccountGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Set permissions
    #region Set partitions

    private static IEndpointRouteBuilder MapSetServiceAccountPartitions(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{serviceAccountGuid:guid}/partitions", SetPartitions)
            .WithName("SetServiceAccountPartitions")
            .WithSummary("Changes the partitions of a service account")
            .WithDescription("Adds and removes the partitions the account belongs to (membership) and the partitions it can access in one request. At least one is required. The caller needs access to every partition it touches. Requires the ChangeServiceAccountPartitions permission, which is separate from the edit permission.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<NoContent> SetPartitions(
        Guid serviceAccountGuid,
        PartitionsChangeDto request,
        ICommandHandler<ServiceAccountSetPartitionsCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ServiceAccountSetPartitionsCommand(serviceAccountGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }
    #endregion Set partitions

    #region Rotate secret

    private static IEndpointRouteBuilder MapRotateServiceAccountSecret(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/{serviceAccountGuid:guid}/rotate-secret", RotateSecret)
            .WithName("RotateServiceAccountSecret")
            .WithSummary("Rotates a service account's secret")
            .WithDescription("Generates a new client secret (returned EXACTLY ONCE) and invalidates all tokens previously issued to the account. Requires the RotateServiceAccountSecret permission, which is separate from the edit permission.")
            .Produces<ServiceAccountCreatedDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        return builder;
    }

    private static async Task<Ok<ServiceAccountCreatedDto>> RotateSecret(
        Guid serviceAccountGuid,
        ICommandHandler<ServiceAccountRotateSecretCommand, ServiceAccountCreatedDto> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.HandleAsync(new ServiceAccountRotateSecretCommand(serviceAccountGuid), cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion

    #region Delete

    private static IEndpointRouteBuilder MapDeleteServiceAccount(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{serviceAccountGuid:guid}", Delete)
            .WithName("DeleteServiceAccount")
            .WithSummary("Deletes a service account")
            .WithDescription("Deletes the specified service account. Any tokens it holds stop validating.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> Delete(
        Guid serviceAccountGuid,
        ICommandHandler<ServiceAccountDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new ServiceAccountDeleteCommand(serviceAccountGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion
}
