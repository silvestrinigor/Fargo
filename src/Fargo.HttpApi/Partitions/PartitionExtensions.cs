using Fargo.Application;
using Fargo.Application.Partitions;
using Fargo.HttpApi.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Fargo.HttpApi.Partitions;

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
            .Produces<HttpContracts.PartitionDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound
        );

        return builder;
    }

    private static async Task<Results<Ok<HttpContracts.PartitionDto>, NotFound>> GetSinglePartition(
        Guid partitionGuid,
        DateTimeOffset? temporalAsOf,
        IQueryHandler<PartitionSingleQuery, PartitionDto?> handler,
        CancellationToken cancellationToken
    )
    {
        var query = new PartitionSingleQuery(partitionGuid, temporalAsOf);

        var response = await handler.Handle(query, cancellationToken);

        return response is null ? TypedResults.NotFound() : TypedResults.Ok(response.ToContract());
    }

    #endregion Get Single

    #region Get Many

    private static IEndpointRouteBuilder MapGetPartitions(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", GetManyPartition)
            .WithName("GetPartitions")
            .WithSummary("Gets multiple partitions")
            .WithDescription("Retrieves a paginated list of partitions. Supports optional temporal queries.")
            .Produces<IReadOnlyCollection<HttpContracts.PartitionDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status204NoContent
        );

        return builder;
    }

    private static async Task<Results<Ok<IReadOnlyCollection<HttpContracts.PartitionDto>>, NoContent>> GetManyPartition(
        DateTimeOffset? temporalAsOfDateTime,
        int? page,
        int? limit,
        [FromQuery] Guid[]? childOfAnyOfThesePartitions,
        bool? notChildOfAnyPartition,
        IQueryHandler<PartitionsQuery, IReadOnlyCollection<PartitionDto>> handler,
        CancellationToken cancellationToken
    )
    {
        var withPagination = new Pagination(
            new Page(page ?? Page.FirstPage.Value),
            new Limit(limit ?? Limit.MaxLimit.Value));

        var query = new PartitionsQuery(
            withPagination,
            temporalAsOfDateTime,
            childOfAnyOfThesePartitions,
            notChildOfAnyPartition
        );

        var response = await handler.Handle(query, cancellationToken);

        if (response.Count == 0)
        {
            return TypedResults.NoContent();
        }

        IReadOnlyCollection<HttpContracts.PartitionDto> contractResponse =
            response.Select(static partition => partition.ToContract()).ToArray();

        return TypedResults.Ok(contractResponse);
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
        HttpContracts.PartitionCreateRequest request,
        ICommandHandler<PartitionCreateCommand, Guid> handler,
        CancellationToken cancellationToken)
    {
        var response = await handler.Handle(new PartitionCreateCommand(request.ToApplication()), cancellationToken);

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
        HttpContracts.PartitionUpdateRequest request,
        ICommandHandler<PartitionUpdateCommand> handler,
        CancellationToken cancellationToken)
    {
        await handler.Handle(new PartitionUpdateCommand(partitionGuid, request.ToApplication()), cancellationToken);

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
