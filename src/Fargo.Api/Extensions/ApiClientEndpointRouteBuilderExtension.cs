using Fargo.Api.Contracts;
using Fargo.Sdk.Contracts.ApiClients;
using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.ApiClients;
using Fargo.Domain;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

public static class ApiClientEndpointRouteBuilderExtension
{
    public static void MapFargoApiClient(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/api-clients")
            .RequireAuthorization()
            .WithTags("ApiClients");

        group.MapGet("/", GetManyApiClients)
            .WithName("GetApiClients")
            .WithSummary("Gets all API clients")
            .Produces<IReadOnlyCollection<ApiClientDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapGet("/{apiClientGuid:guid}", GetSingleApiClient)
            .WithName("GetApiClient")
            .WithSummary("Gets a single API client")
            .Produces<ApiClientDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapPost("/", CreateApiClient)
            .WithName("CreateApiClient")
            .WithSummary("Creates a new API client and returns its key (shown once)")
            .Produces<ApiClientCreatedDto>(StatusCodes.Status200OK);

        group.MapPatch("/{apiClientGuid:guid}", UpdateApiClient)
            .WithName("UpdateApiClient")
            .WithSummary("Updates an API client")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{apiClientGuid:guid}", DeleteApiClient)
            .WithName("DeleteApiClient")
            .WithSummary("Deletes an API client")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<Results<Ok<IReadOnlyCollection<ApiClientDto>>, NoContent>> GetManyApiClients(
        IQueryHandler<ApiClientManyQuery, IReadOnlyCollection<ApiClientInformation>> handler,
        Page? page,
        Limit? limit,
        string? search,
        CancellationToken cancellationToken)
    {
        var query = new ApiClientManyQuery(
            Pagination: PaginationHelpers.CreatePagination(page, limit),
            Search: search);
        var result = await handler.Handle(query, cancellationToken);
        if (result.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<ApiClientDto>>(result.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<Results<Ok<ApiClientDto>, NotFound>> GetSingleApiClient(
        Guid apiClientGuid,
        IQueryHandler<ApiClientSingleQuery, ApiClientInformation?> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(new ApiClientSingleQuery(apiClientGuid), cancellationToken);
        return result is null ? TypedResults.NotFound() : TypedResults.Ok(result.ToContract());
    }

    private static async Task<Ok<ApiClientCreatedDto>> CreateApiClient(
        ApiClientCreateRequest request,
        ICommandHandler<ApiClientCreateCommand, ApiClientCreatedResult> handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.Handle(request.ToCommand(), cancellationToken);
        return TypedResults.Ok(result.ToContract());
    }

    private static async Task<Results<NoContent, NotFound>> UpdateApiClient(
        Guid apiClientGuid,
        ApiClientUpdateRequest request,
        ICommandHandler<ApiClientUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(apiClientGuid), cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Results<NoContent, NotFound>> DeleteApiClient(
        Guid apiClientGuid,
        ICommandHandler<ApiClientDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new ApiClientDeleteCommand(apiClientGuid), cancellationToken);
        return TypedResults.NoContent();
    }
}
