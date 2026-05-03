using Fargo.Application;
using Fargo.Application.Partitions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Fargo.Api.Partitions;

public static class PartitionEndpointRouteBuilderExtension
{
    public static void MapFargoPartition(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapPartitionGroup();

        group.MapGetPartition();

        group.MapGetPartitions();

        group.MapCreatePartition();

        group.MapUpdatePartition();

        group.MapDeletePartition();
    }

    private static RouteGroupBuilder MapPartitionGroup(this IEndpointRouteBuilder builder)
    {
        var group = builder
            .MapGroup("/partitions")
            .RequireAuthorization()
            .WithTags("Partitions");

        return group;
    }

    #region Get Single

    private static IEndpointRouteBuilder MapGetPartition(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/{partitionGuid:guid}", GetSinglePartition)
            .WithName("GetPartition")
            .WithSummary("Gets a single partition")
            .WithDescription("Retrieves a single partition by its unique identifier. Optionally allows querying historical data using temporal tables.")
            .Produces<PartitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<PartitionDto>, NotFound>> GetSinglePartition(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<PartitionSingleQuery, PartitionDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response);
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetPartitions(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyPartition)
            .WithName("GetPartitions")
            .WithSummary("Gets multiple partitions")
            .WithDescription("Retrieves a paginated list of partitions. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<PartitionDto>>, NoContent>> GetManyPartition(
        DateTimeOffset? temporalAsOfDateTime,
        Page? page,
        Limit? limit,
        IReadOnlyCollection<Guid>? insideAnyOfThisPartitions,
        bool? notInsideAnyPartition,
        IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(page ?? Page.FirstPage, limit ?? Limit.MaxLimit);

        var query = new PartitionsQuery(
            withPagination,
            temporalAsOfDateTime,
            insideAnyOfThisPartitions,
            notInsideAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        return TypedResults.Ok(response);
    }

    #endregion Get Many

    #region Create

    private static IEndpointRouteBuilder MapCreatePartition(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/", CreatePartition)
            .WithName("CreatePartition")
            .WithSummary("Creates a new partition")
            .WithDescription("Creates a new partition under an optional parent (defaults to the global partition). Returns the generated identifier.")
            .Produces<Guid>(StatusCodes.Status200OK);

        return builder;
    }

    private static async Task<Ok<Guid>> CreatePartition(
        PartitionCreateCommand request,
        ICommandHandler<PartitionCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(request, cancellationToken);

        return TypedResults.Ok(response);
    }

    #endregion Create

    #region Update

    private static IEndpointRouteBuilder MapUpdatePartition(this IEndpointRouteBuilder builder)
    {
        builder.MapPut("/{partitionGuid:guid}", UpdatePartition)
            .WithName("UpdatePartition")
            .WithSummary("Updates an existing partition")
            .WithDescription("Updates partition properties. Only fields provided in the body are applied; null fields are ignored.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> UpdatePartition(
        Guid partitionGuid,
        PartitionUpdateDto request,
        ICommandHandler<PartitionUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PartitionUpdateCommand(partitionGuid, request), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Update

    #region Delete

    private static IEndpointRouteBuilder MapDeletePartition(this IEndpointRouteBuilder builder)
    {
        builder.MapDelete("/{partitionGuid:guid}", DeletePartition)
            .WithName("DeletePartition")
            .WithSummary("Deletes a partition")
            .WithDescription("Deletes the specified partition from the system. The global partition cannot be deleted.")
            .Produces(StatusCodes.Status204NoContent);

        return builder;
    }

    private static async Task<NoContent> DeletePartition(
        Guid partitionGuid,
        ICommandHandler<PartitionDeleteCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PartitionDeleteCommand(partitionGuid), cancellationToken);

        return TypedResults.NoContent();
    }

    #endregion Delete
}
