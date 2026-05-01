using Fargo.Api.Contracts;
using Fargo.Api.Helpers;
using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.Domain;
using Fargo.Sdk.Contracts.Partitions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Extensions;

/// <summary>
/// Extension responsible for mapping all Partition endpoints.
/// </summary>
public static class PartitionEndpointRouteBuilderExtension
{
    /// <summary>
    /// Maps all routes related to partitions.
    /// </summary>
    /// <param name="builder">The endpoint route builder.</param>
    public static void MapFargoPartition(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/partitions")
            .RequireAuthorization()
            .WithTags("Partitions");

        group.MapGet("/{partitionGuid:guid}", GetSinglePartition)
            .WithName("GetPartition")
            .WithSummary("Gets a single partition")
            .WithDescription("Retrieves a single partition by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<PartitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/", GetManyPartition)
            .WithName("GetPartitions")
            .WithSummary("Gets multiple partitions")
            .WithDescription("Retrieves a paginated list of partitions. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/", CreatePartition)
            .WithName("CreatePartition")
            .WithSummary("Creates a new partition")
            .WithDescription("Creates a new partition and returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        group.MapPatch("/{partitionGuid:guid}", UpdatePartition)
            .WithName("UpdatePartition")
            .WithSummary("Updates an existing partition")
            .WithDescription("Updates a partition using partial data.")
            .Produces(StatusCodes.Status204NoContent);

        group.MapDelete("/{partitionGuid:guid}", DeletePartition)
            .WithName("DeletePartition")
            .WithSummary("Deletes a partition")
            .WithDescription("Deletes the specified partition from the system.")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<Results<Ok<PartitionDto>, NotFound>> GetSinglePartition(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<PartitionSingleQuery, PartitionInformation?> handler,
        CancellationToken cancellationToken)
    {
        var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionDto>>, NoContent>> GetManyPartition(
        Guid? parentPartitionGuid,
        DateTimeOffset? temporalAsOf,
        Page? page,
        Limit? limit,
        bool? rootOnly,
        string? search,
        IQueryHandler<PartitionManyQuery, IReadOnlyCollection<PartitionInformation>> handler,
        CancellationToken cancellationToken)
    {
        var query = new PartitionManyQuery(
            parentPartitionGuid,
            temporalAsOf,
            PaginationHelpers.CreatePagination(page, limit),
            rootOnly ?? false,
            search
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok<IReadOnlyCollection<PartitionDto>>(response.Select(x => x.ToContract()).ToArray());
    }

    private static async Task<Ok<Guid>> CreatePartition(
        PartitionCreateRequest request,
        ICommandHandler<PartitionCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request.ToCommand(), cancellationToken);

        return TypedResults.Ok(response);
    }

    private static async Task<NoContent> UpdatePartition(
        Guid partitionGuid,
        PartitionUpdateRequest request,
        ICommandHandler<PartitionUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(request.ToCommand(partitionGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    private static async Task<NoContent> DeletePartition(
        Guid partitionGuid,
        ICommandHandler<PartitionDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        var command = new PartitionDeleteCommand(partitionGuid);

        await handler.Handle(command, cancellationToken);

        return TypedResults.NoContent();
    }
}
